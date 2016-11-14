using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace HL7Comparer
{
    public class TextMarkerService : IBackgroundRenderer, IVisualLineTransformer, ITextMarkerService
    {
        private readonly TextEditor _textEditor;
        private readonly TextSegmentCollection<TextMarker> _markers;

        public sealed class TextMarker : TextSegment
        {
            public TextMarker(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;
            }

            public string ToolTip { get; set; }
        }

        public TextMarkerTypes MarkerType { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }

        public TextMarkerService(TextEditor textEditor)
        {
            _textEditor = textEditor;
            _markers = new TextSegmentCollection<TextMarker>(textEditor.Document);
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_markers == null || !textView.VisualLinesValid)
            {
                return;
            }
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
            {
                return;
            }
            var viewStart = visualLines.First().FirstDocumentLine.Offset;
            var viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                if (BackgroundColor != null)
                {
                    BackgroundGeometryBuilder geoBuilder = new BackgroundGeometryBuilder
                    {
                        AlignToWholePixels = true,
                        CornerRadius = 3
                    };
                    geoBuilder.AddSegment(textView, marker);
                    Geometry geometry = geoBuilder.CreateGeometry();
                    if (geometry != null)
                    {
                        drawingContext.DrawGeometry(BackgroundColor, null, geometry);
                    }
                }
                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;
                    if ((MarkerType & TextMarkerTypes.SquigglyUnderline) != 0)
                    {
                        double offset = 2.5;
                        int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);
                        StreamGeometry geometry = new StreamGeometry();
                        using (StreamGeometryContext ctx = geometry.Open())
                        {
                            ctx.BeginFigure(startPoint, false, false);
                            ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                        }
                        geometry.Freeze();
                        Pen usedPen = new Pen(ForegroundColor, 1);
                        usedPen.Freeze();
                        drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    }
                    if ((MarkerType & TextMarkerTypes.NormalUnderline) != 0)
                    {
                        Pen usedPen = new Pen(ForegroundColor, 1);
                        usedPen.Freeze();
                        drawingContext.DrawLine(usedPen, startPoint, endPoint);
                    }
                    if ((MarkerType & TextMarkerTypes.DottedUnderline) != 0)
                    {
                        Pen usedPen = new Pen(ForegroundColor, 1) {DashStyle = DashStyles.Dot};
                        usedPen.Freeze();
                        drawingContext.DrawLine(usedPen, startPoint, endPoint);
                    }
                    if ((MarkerType & TextMarkerTypes.Rectangle) != 0)
                    {
                        Pen usedPen = new Pen(ForegroundColor, 1);
                        usedPen.Freeze();
                        drawingContext.DrawRectangle(null, usedPen, r);
                    }
                }
            }
        }

        public KnownLayer Layer => KnownLayer.Selection;

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }

        public void Clear()
        {
            _markers.Clear();
            _textEditor.TextArea.TextView.InvalidateLayer(Layer);
        }

        private void Remove(TextMarker marker)
        {
            if (_markers.Remove(marker))
            {
                Redraw(marker);
            }
        }

        private void Redraw(ISegment segment)
        {
            _textEditor.TextArea.TextView.Redraw(segment);
        }

        public void AddMarker(int offset, int length, string message)
        {
            var m = new TextMarker(offset, length);
            _markers.Add(m);
            m.ToolTip = message;
            Redraw(m);
        }

        public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
        {
            return _markers == null ? Enumerable.Empty<TextMarker>() : _markers.FindSegmentsContaining(offset);
        }

        /// <summary>
        /// Applies the transformation to the specified list of visual line elements.
        /// </summary>
        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
        {
            
        }
    }
}
