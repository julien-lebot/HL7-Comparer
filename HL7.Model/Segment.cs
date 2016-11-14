using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Segment
    {
        public string Name { get; }
        public int LineNumber { get; }
        public Dictionary<int, Field> Fields { get; set; }

        public IEnumerable<Component> Components
        {
            get { return Fields.Values.SelectMany(f => f.Components.Values); }
        }

        public Segment(string name, int lineNumber)
        {
            Name = name;
            LineNumber = lineNumber;
        }
    }
}