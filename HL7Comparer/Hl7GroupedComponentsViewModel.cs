using System.Collections.Generic;
using ReactiveUI;

namespace HL7Comparer
{
    public class Hl7GroupedComponentsViewModel
    {
        public Hl7GroupedComponentsViewModel(string segmentName, IEnumerable<Hl7ComponentViewModel> components)
        {
            Components = new ReactiveList<Hl7ComponentViewModel>(components);
            SegmentName = segmentName;
        }

        public string SegmentName { get; }
        public ReactiveList<Hl7ComponentViewModel> Components { get; }
    }
}