using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Component : IEquatable<Component>
    {
        public Segment Segment { get; }
        public int FieldIdx { get; }
        public int ComponentIdx { get; }
        public string Id => $"{Segment.Name}-{FieldIdx}.{ComponentIdx}";
        public List<string> Values { get; } = new List<string>();

        public Component(Segment segment, int fieldIdx, int componentIdx)
        {
            Segment = segment;
            FieldIdx = fieldIdx;
            ComponentIdx = componentIdx;
        }

        public bool Equals(Component other)
        {
            return other != null &&
                   Id == other.Id &&
                   Values.SequenceEqual(other.Values);
        }

        public override string ToString()
        {
            return string.Join("^", Values);
        }
    }
}