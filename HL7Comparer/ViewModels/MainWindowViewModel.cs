using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using HL7Comparer.Services;
using MaterialDesignThemes.Wpf;
using ReactiveUI;

namespace HL7Comparer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ReactiveList<IDifference> _differences = new ReactiveList<IDifference>();
        private readonly ObservableAsPropertyHelper<int> _differencesCount;
        private readonly IUserPreferencesService _userPreferencesService;
        private string _differencesAsText;
        private bool _displayLineNumber;
        private ReactiveList<StringViewModel> _idsToIgnore = new ReactiveList<StringViewModel>();

        public MainWindowViewModel(IHL7Editor leftHl7Editor,
            IHL7Editor rightHl7Editor,
            IUserPreferencesService userPreferencesService,
            IMessagesComparer messagesComparer,
            ISnackbarMessageQueue messageQueue)
        {
            _userPreferencesService = userPreferencesService;
            MessageQueue = messageQueue;
            LeftHL7Editor = leftHl7Editor;
            RightHL7Editor = rightHl7Editor;
            LeftHL7Editor.LoadFromCache("LeftEditorCache");
            RightHL7Editor.LoadFromCache("RightEditorCache");

            Observable.FromEventPattern(
                h => _userPreferencesService.PreferencesSaved += h,
                h => _userPreferencesService.PreferencesSaved -= h)
                .Subscribe(_ => MessageQueue.Enqueue("Settings saved"));

            SaveIdsCommand = ReactiveCommand.Create<object>(_ =>
            {
                _userPreferencesService.Set("IdsToIgnore", _idsToIgnore.Select(s => s.Value).ToList());
            });
            SaveIdsCommand.ThrownExceptions.Subscribe(ex => MessageQueue.Enqueue(ex.ToString()));

            DeleteIdCommand = ReactiveCommand.Create<StringViewModel>(s => { IdsToIgnore.Remove(s); });
            DeleteIdCommand.ThrownExceptions.Subscribe(ex => MessageQueue.Enqueue(ex.ToString()));

            AddItemCommand = ReactiveCommand.Create<object>(_ => { IdsToIgnore.Add(new StringViewModel(string.Empty)); });
            AddItemCommand.ThrownExceptions.Subscribe(ex => MessageQueue.Enqueue(ex.ToString()));

            DiscardIdsChangesCommand = ReactiveCommand.Create<object>(_ => { LoadIdsToIgnore(); });
            DiscardIdsChangesCommand.ThrownExceptions.Subscribe(ex => MessageQueue.Enqueue(ex.ToString()));

            var whenMessagesChanged =
                this.WhenAnyValue(vm => vm.LeftHL7Editor.HL7Message, vm => vm.RightHL7Editor.HL7Message);

            var canCompareMessages =
                whenMessagesChanged
                    .Select(msgs => msgs.Item1 != null && msgs.Item1.Segments.Any() &&
                                    msgs.Item2 != null && msgs.Item2.Segments.Any());

            CompareHL7MessageCommand = ReactiveCommand.Create<object>(_ =>
            {
                using (_differences.SuppressChangeNotifications())
                {
                    _differences.Clear();
                    _differences.AddRange(messagesComparer.Compare(LeftHL7Editor.HL7Message,
                        RightHL7Editor.HL7Message,
                        _idsToIgnore.Select(s => s.Value).ToList()));
                }
            }, canCompareMessages);
            CompareHL7MessageCommand.ThrownExceptions.Subscribe(ex => MessageQueue.Enqueue(ex.ToString()));
            SaveIdsCommand.IsExecuting.Where(x => x).InvokeCommand(CompareHL7MessageCommand);
            whenMessagesChanged.InvokeCommand(CompareHL7MessageCommand);

            _differencesCount = Differences.CountChanged.ToProperty(this, vm => vm.DifferencesCount);
            Differences.Changed.Subscribe(x => UpdateDifferences());

            LoadPreferences();
            this.WhenAnyValue(vm => vm.DisplayLineNumber)
                .Skip(1)
                .Subscribe(value =>
                {
                    _userPreferencesService.Set("DisplayLineNumber", value);
                });
        }

        private void UpdateDifferences()
        {
            LeftHL7Editor.ClearMarkers();
            RightHL7Editor.ClearMarkers();

            var differencesBuilder = new StringBuilder();
            foreach (var diff in Differences)
            {
                var msg = string.Empty;
                if (diff is ComponentValueDifference)
                {
                    var cvd = diff as ComponentValueDifference;
                    LeftHL7Editor.AddHL7ComponentMarker(cvd.Source, cvd.AsText());
                    RightHL7Editor.AddHL7ComponentMarker(cvd.Target, cvd.AsText());
                    msg = cvd.AsText();
                }
                else if (diff is MissingComponentDifference)
                {
                    var mcd = diff as MissingComponentDifference;
                    var location = mcd.DifferenceLocation == DifferenceLocation.Target
                        ? "Source"
                        : "Destination";
                    msg = $"At line {mcd.Source.ParentSegment.LineNumber} in {location}: {mcd.AsText()}";
                    RightHL7Editor.AddHL7ComponentMarker(mcd.Source, mcd.AsText());
                    LeftHL7Editor.AddHL7ComponentMarker(mcd.Source, mcd.AsText());
                }
                else if (diff is MissingSegmentDifference)
                {
                    var msd = diff as MissingSegmentDifference;
                    var location = msd.MissingSegmentDifferenceLocation == DifferenceLocation.Target
                        ? "Source"
                        : "Destination";
                    msg = $"At line {msd.MissingSegment.LineNumber} in {location}: {msd.AsText()}";
                }
                differencesBuilder.AppendLine(msg);
            }
            DifferencesAsText = differencesBuilder.ToString();
            LeftHL7Editor.SaveInCache("LeftEditorCache");
            RightHL7Editor.SaveInCache("RightEditorCache");
        }

        private ReactiveCommand CompareHL7MessageCommand { get; }

        public IHL7Editor LeftHL7Editor { get; }
        public IHL7Editor RightHL7Editor { get; }

        public IReadOnlyReactiveCollection<IDifference> Differences => _differences;

        public int DifferencesCount => _differencesCount.Value;

        public string DifferencesAsText
        {
            get { return _differencesAsText; }
            set { this.RaiseAndSetIfChanged(ref _differencesAsText, value); }
        }

        public bool DisplayLineNumber
        {
            get { return _displayLineNumber; }
            set { this.RaiseAndSetIfChanged(ref _displayLineNumber, value); }
        }

        public ReactiveList<StringViewModel> IdsToIgnore
        {
            get { return _idsToIgnore; }
            set { this.RaiseAndSetIfChanged(ref _idsToIgnore, value); }
        }

        public ReactiveCommand SaveIdsCommand { get; }
        public ReactiveCommand DiscardIdsChangesCommand { get; }
        public ReactiveCommand DeleteIdCommand { get; }
        public ReactiveCommand AddItemCommand { get; }

        public ISnackbarMessageQueue MessageQueue { get; }

        private void LoadPreferences()
        {
            _userPreferencesService.TryGetPreference("DisplayLineNumber", ref _displayLineNumber);
            LoadIdsToIgnore();
        }

        private void LoadIdsToIgnore()
        {
            var idsToIgnore = new List<string>();
            if (!_userPreferencesService.TryGetPreference("IdsToIgnore", idsToIgnore))
            {
                using (_idsToIgnore.SuppressChangeNotifications())
                {
                    _idsToIgnore.Clear();
                    _idsToIgnore.AddRange(new[]
                    {
                        "MSH-7.1", // DateTime
                        "MSH-10.1", // ControlID

                        "OBR-7.1", // DateTime
                        "OBR-13.1", // NeuronID

                        "OBX-1.1",
                        "OBX-14.1",
                        "OBX-15.1"
                    }.Select(s => new StringViewModel(s)));
                }
            }
            else
            {
                using (_idsToIgnore.SuppressChangeNotifications())
                {
                    _idsToIgnore.Clear();
                    _idsToIgnore.AddRange(idsToIgnore.Select(s => new StringViewModel(s)));
                }
            }
        }
    }
}