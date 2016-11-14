using System.Collections.Generic;

namespace HL7Comparer
{
    public class Field
    {
        public Dictionary<int, Component> Components { get; } = new Dictionary<int, Component>();
    }
}