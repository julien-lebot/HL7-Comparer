using ReactiveUI;

namespace HL7Comparer
{
    public class StringViewModel : ReactiveObject
    {
        private string _model;

        public StringViewModel(string value)
        {
            _model = value;
        }

        public string Value
        {
            get { return _model; }
            set { this.RaiseAndSetIfChanged(ref _model, value); }
        }
    }
}