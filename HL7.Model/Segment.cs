using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Segment
    {
        public Segment(string name, int lineNumber)
        {
            Name = name;
            LineNumber = lineNumber;
            Fields = new IndexedList<int, Field>(f => f.Index, fieldIndex => new Field(this, fieldIndex));
        }

        public string Name { get; }
        public int LineNumber { get; }

        public IIndexedList<int, Field> Fields { get; }

        public IEnumerable<Component> Components
        {
            get { return Fields.SelectMany(f => f.RepeatedFields.SelectMany(rf => rf.Components)); }
        }
    }
}