using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Component : IEquatable<Component>
    {
        public Component(Segment parentSegment, int fieldIdx, int componentIdx)
        {
            ParentSegment = parentSegment;
            FieldIdx = fieldIdx;
            ComponentIdx = componentIdx;
        }

        public Segment ParentSegment { get; }
        public int FieldIdx { get; }
        public int ComponentIdx { get; }
        public string Id => $"{ParentSegment.Name}-{FieldIdx}.{ComponentIdx}";
        public List<string> Values { get; } = new List<string>();

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