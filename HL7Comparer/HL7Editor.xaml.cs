using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace HL7Comparer
{
    /// <summary>
    /// Interaction logic for HL7Editor.xaml
    /// </summary>
    public partial class HL7Editor : UserControl
    {
        private readonly TextMarkerService _textMarkerService;
        private ToolTip _toolTip;

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
            "Document", typeof(TextDocument), typeof(HL7Editor), new PropertyMetadata(new TextDocument(), DocumentChanged));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        private static void DocumentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = (HL7Editor) dependencyObject;
            var document = (TextDocument) dependencyPropertyChangedEventArgs.NewValue;
            if (document != null)
            {
                var documentContainer = new ServiceContainer();
                documentContainer.AddService(typeof(IDocument), document);
                documentContainer.AddService(typeof(TextDocument), document);
                documentContainer.AddService(typeof(ITextMarkerService), self._textMarkerService);
                document.ServiceProvider = documentContainer;
            }

        }

        public static readonly DependencyProperty MarkerFillColorProperty = DependencyProperty.Register(
            "MarkerFillColor", typeof(SolidColorBrush), typeof(HL7Editor), new PropertyMetadata(default(SolidColorBrush), MarkerFillColorChanged));

        private static void MarkerFillColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = (HL7Editor)dependencyObject;
            self._textMarkerService.BackgroundColor = (SolidColorBrush)dependencyPropertyChangedEventArgs.NewValue;
        }

        public SolidColorBrush MarkerFillColor
        {
            get { return (SolidColorBrush) GetValue(MarkerFillColorProperty); }
            set { SetValue(MarkerFillColorProperty, value); }
        }

        public static readonly DependencyProperty MarkerStrokeColorProperty = DependencyProperty.Register(
            "MarkerStrokeColor", typeof(SolidColorBrush), typeof(HL7Editor), new PropertyMetadata(default(SolidColorBrush), MarkerStrokeColorChanged));

        private static void MarkerStrokeColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = (HL7Editor)dependencyObject;
            self._textMarkerService.ForegroundColor = (SolidColorBrush)dependencyPropertyChangedEventArgs.NewValue;
        }

        public SolidColorBrush MarkerStrokeColor
        {
            get { return (SolidColorBrush) GetValue(MarkerStrokeColorProperty); }
            set { SetValue(MarkerStrokeColorProperty, value); }
        }

        public static readonly DependencyProperty TextMarkerTypeProperty = DependencyProperty.Register(
            "TextMarkerType", typeof(TextMarkerTypes), typeof(HL7Editor), new PropertyMetadata(default(TextMarkerTypes), TextMarkerTypeChanged));

        private static void TextMarkerTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = (HL7Editor)dependencyObject;
            self._textMarkerService.MarkerType = (TextMarkerTypes)dependencyPropertyChangedEventArgs.NewValue;
        }

        public TextMarkerTypes TextMarkerType
        {
            get { return (TextMarkerTypes) GetValue(TextMarkerTypeProperty); }
            set { SetValue(TextMarkerTypeProperty, value); }
        }

        public static readonly DependencyProperty IsModifiedProperty = DependencyProperty.Register(
            "IsModified", typeof(bool), typeof(HL7Editor), new PropertyMetadata(default(bool)));

        public bool IsModified
        {
            get { return (bool) GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        public static readonly DependencyProperty ShowLineNumbersProperty = DependencyProperty.Register(
            "ShowLineNumbers", typeof(bool), typeof(HL7Editor), new PropertyMetadata(default(bool)));

        public bool ShowLineNumbers
        {
            get { return (bool) GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(string), typeof(HL7Editor), new PropertyMetadata(default(string)));

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public HL7Editor()
        {
            InitializeComponent();

            _textMarkerService = new TextMarkerService(TextEditor);
            TextView textView = TextEditor.TextArea.TextView;
            textView.BackgroundRenderers.Add(_textMarkerService);
            textView.LineTransformers.Add(_textMarkerService);
            textView.MouseHover += MouseHover;
            textView.MouseHoverStopped += TextEditorMouseHoverStopped;
            textView.VisualLinesChanged += VisualLinesChanged;
        }

        private void MouseHover(object sender, MouseEventArgs e)
        {
            var pos = TextEditor.TextArea.TextView.GetPositionFloor(e.GetPosition(TextEditor.TextArea.TextView) + TextEditor.TextArea.TextView.ScrollOffset);
            bool inDocument = pos.HasValue;
            if (inDocument)
            {
                TextLocation logicalPosition = pos.Value.Location;
                int offset = TextEditor.Document.GetOffset(logicalPosition);
                
                var markersAtOffset = _textMarkerService.GetMarkersAtOffset(offset);
                TextMarkerService.TextMarker markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip != null)
                {
                    if (_toolTip == null)
                    {
                        _toolTip = new ToolTip();
                        _toolTip.Closed += ToolTipClosed;
                        _toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                        _toolTip.Content = new TextBlock
                        {
                            Text = markerWithToolTip.ToolTip,
                            TextWrapping = TextWrapping.Wrap
                        };
                        _toolTip.IsOpen = true;
                        e.Handled = true;
                    }
                }
            }
        }

        void ToolTipClosed(object sender, RoutedEventArgs e)
        {
            _toolTip = null;
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
                e.Handled = true;
            }
        }

        private void VisualLinesChanged(object sender, EventArgs e)
        {
            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
            }
        }
    }
}
