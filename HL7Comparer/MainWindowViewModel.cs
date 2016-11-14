using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Windows.Input;
using ReactiveUI;

namespace HL7Comparer
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IUserPreferencesService _userPreferencesService;
        private readonly ReactiveList<IDifference> _differences = new ReactiveList<IDifference>();
        private ReactiveList<StringViewModel> _idsToIgnore = new ReactiveList<StringViewModel>();
        private readonly ObservableAsPropertyHelper<int> _differencesCount;
        private bool _displayLineNumber;
        private string _differencesAsText;

        private ICommand CompareHL7MessageCommand { get; }

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
            set
            {
                this.RaiseAndSetIfChanged(ref _displayLineNumber, value);
                _userPreferencesService.Set("DisplayLineNumber", value);
            }
        }

        public ReactiveList<StringViewModel> IdsToIgnore
        {
            get { return _idsToIgnore; }
            set { this.RaiseAndSetIfChanged(ref _idsToIgnore, value); }
        }

        public ICommand SaveIdsCommand { get; }
        public ICommand DiscardIdsChangesCommand { get; }
        public ICommand DeleteIdCommand { get; }
        public ICommand AddItemCommand { get; }

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
                        "MSH-6.1", // DateTime
                        "MSH-9.1", // ControlID

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

        public MainWindowViewModel(IHL7Editor leftHl7Editor, IHL7Editor rightHl7Editor, IUserPreferencesService userPreferencesService, IMessagesComparer messagesComparer)
        {
            _userPreferencesService = userPreferencesService;
            LeftHL7Editor = leftHl7Editor;
            RightHL7Editor = rightHl7Editor;
            LeftHL7Editor.LoadFromCache("LeftEditorCache");
            RightHL7Editor.LoadFromCache("RightEditorCache");

            SaveIdsCommand = ReactiveCommand.Create<object>(_ =>
            {
                _userPreferencesService.Set("IdsToIgnore", _idsToIgnore.Select(s => s.Value).ToList());
                if (CompareHL7MessageCommand.CanExecute(null))
                {
                    CompareHL7MessageCommand.Execute(null);
                }
            });

            DeleteIdCommand = ReactiveCommand.Create<StringViewModel>(s =>
            {
                IdsToIgnore.Remove(s);
            });

            AddItemCommand = ReactiveCommand.Create<object>(_ =>
            {
                IdsToIgnore.Add(new StringViewModel(string.Empty));
            });

            DiscardIdsChangesCommand = ReactiveCommand.Create<object>(_ =>
            {
                LoadIdsToIgnore();
            });

            CompareHL7MessageCommand = ReactiveCommand.Create<object>(_ =>
            {
                using (_differences.SuppressChangeNotifications())
                {
                    _differences.Clear();
                    _differences.AddRange(messagesComparer.Compare(LeftHL7Editor.HL7Message, RightHL7Editor.HL7Message, _idsToIgnore.Select(s => s.Value).ToList()));
                }
            },
            this.WhenAnyValue(vm => vm.LeftHL7Editor.HL7Message, vm => vm.RightHL7Editor.HL7Message)
                .Select(msgs => msgs.Item1 != null && msgs.Item1.Segments.Any() &&
                                msgs.Item2 != null && msgs.Item2.Segments.Any()));

            this.WhenAnyValue(vm => vm.LeftHL7Editor.HL7Message, vm => vm.RightHL7Editor.HL7Message)
                .InvokeCommand(CompareHL7MessageCommand);

            _differencesCount = Differences.CountChanged.ToProperty(this, vm => vm.DifferencesCount);
            Differences.Changed
                       .Subscribe(x =>
                       {
                           LeftHL7Editor.ClearMarkers();
                           RightHL7Editor.ClearMarkers();

                           var differencesBuilder = new StringBuilder();
                           foreach (var diff in Differences)
                           {
                               string msg = string.Empty;
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
                                   var location = mcd.DifferenceLocation == DifferenceLocation.Target ? "Source" : "Destination";
                                   msg = $"At line {(mcd.Source.Segment.LineNumber)} in {location}: {mcd.AsText()}";
                                   RightHL7Editor.AddHL7ComponentMarker(mcd.Source, mcd.AsText());
                                   LeftHL7Editor.AddHL7ComponentMarker(mcd.Source, mcd.AsText());

                               }
                               else if (diff is MissingSegmentDifference)
                               {
                                   var msd = diff as MissingSegmentDifference;
                                   var location = msd.MissingSegmentDifferenceLocation == DifferenceLocation.Target ? "Source" : "Destination";
                                   msg = $"At line {msd.MissingSegment.LineNumber} in {location}: {msd.AsText()}";
                               }
                               differencesBuilder.AppendLine(msg);
                           }
                           DifferencesAsText = differencesBuilder.ToString();
                           LeftHL7Editor.SaveInCache("LeftEditorCache");
                           RightHL7Editor.SaveInCache("RightEditorCache");
                       });
            LoadPreferences();
        }
    }
}
