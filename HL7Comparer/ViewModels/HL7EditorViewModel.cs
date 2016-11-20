using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HL7Comparer.Extensions;
using HL7Comparer.Services;
using ICSharpCode.AvalonEdit.Document;
using ReactiveUI;

namespace HL7Comparer.ViewModels
{
    public class HL7EditorViewModel : ReactiveObject, IHL7Editor
    {
        private readonly ICacheService _cacheService;
        private readonly TextDocument _document = new TextDocument();
        private Message _HL7Message;
        private bool _isModified;

        public HL7EditorViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            Observable.FromEventPattern<EventHandler<TextChangeEventArgs>, TextChangeEventArgs>(
                h => Document.TextChanged += h,
                h => Document.TextChanged -= h)
            .Subscribe(
                _ =>
                {
                    HL7Message = Message.Parse(Document.Text);
                    IsModified = true;
                });
        }

        private ITextMarkerService TextMarkerService
            => (ITextMarkerService)Document.GetService(typeof(ITextMarkerService));

        public IDocument Document => _document;

        public Message HL7Message
        {
            get { return _HL7Message; }
            private set { this.RaiseAndSetIfChanged(ref _HL7Message, value); }
        }

        public bool IsModified
        {
            get { return _isModified; }
            set { this.RaiseAndSetIfChanged(ref _isModified, value); }
        }

        public void AddHL7ComponentMarker(Component component, string message)
        {
            var isMsh = component.ParentSegment.Name == "MSH";
            var srcLine = Document.GetLineByNumber(component.ParentSegment.LineNumber);
            var srcLineText = Document.GetText(srcLine.Offset, srcLine.TotalLength);
            var indexOfSourceField =
                srcLineText.IndexOfNth(c => c == HL7Message.DefaultSeparator[Separators.FieldSeparator],
                     isMsh? component.FieldIdx - 1 : component.FieldIdx);
            if (!isMsh || component.FieldIdx != 1)
            {
                indexOfSourceField++;
            }
            if (indexOfSourceField < 0)
            {
                indexOfSourceField = 0;
            }
            var sourceFieldLength =
                srcLineText.IndexOf(HL7Message.DefaultSeparator[Separators.FieldSeparator], indexOfSourceField) -
                indexOfSourceField;
            if (sourceFieldLength <= 0)
            {
                sourceFieldLength = 1;
            }
            TextMarkerService.AddMarker(srcLine.Offset + indexOfSourceField, sourceFieldLength, message);
        }

        public void ClearMarkers()
        {
            TextMarkerService.Clear();
        }

        public async Task LoadFromCache(string key)
        {
            if (_cacheService.KeyExists(key))
            {
                Document.Text = await _cacheService.Load<string>(key);
            }
        }

        public async Task SaveInCache(string key)
        {
            await _cacheService.Save(key, Document.Text);
            IsModified = false;
        }
    }
}