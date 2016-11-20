using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public static class CompareUsing
    {
        public static IEqualityComparer<Segment> SegmentName { get; }
            = new EqualityComparer<Segment>(seg => seg.Name.GetHashCode(), (src, dest) => src.Name == dest.Name);

        public static IEqualityComparer<Segment> ObxSegmentComparer { get; }
            = new EqualityComparer<Segment>(seg => seg.GetVariableId().GetHashCode(),
                (src, dest) => src.GetVariableId() == dest.GetVariableId());

        public static IEqualityComparer<Component> ComponentValue { get; }
            = new EqualityComparer<Component>(comp => comp.Id.GetHashCode(),
                (src, dest) => src.Id == dest.Id && src.Values.SequenceEqual(dest.Values));

        private class EqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equals;
            private readonly Func<T, int> _getHashCode;

            public EqualityComparer(Func<T, int> getHashCode, Func<T, T, bool> equals)
            {
                _getHashCode = getHashCode;
                _equals = equals;
            }

            public bool Equals(T x, T y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null) ||
                    ReferenceEquals(y, null))
                {
                    return false;
                }
                return _equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return _getHashCode(obj);
            }
        }
    }
}