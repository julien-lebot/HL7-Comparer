using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public static class HelperExtensions
    {
        public static TR Transform<T1, T2, TR>(this Tuple<IEnumerable<T1>, IEnumerable<T2>> source,
            Func<IEnumerable<T1>, IEnumerable<T2>, TR> fn)
        {
            return fn(source.Item1, source.Item2);
        }

        public static IEnumerable<TR> Transform<T1, T2, TR>(
            this IEnumerable<Tuple<IEnumerable<T1>, IEnumerable<T2>>> source,
            Func<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<TR>> fn)
        {
            return source.SelectMany(x => fn(x.Item1, x.Item2));
        }

        public static IEnumerable<MissingSegmentDifference> AsMissingSegment(this IEnumerable<Segment> segments,
            DifferenceLocation differenceLocation)
        {
            return segments.Select(s => new MissingSegmentDifference(s, differenceLocation));
        }

        public static IEnumerable<MissingComponentDifference> AsMissingComponent(this IEnumerable<Component> components,
            DifferenceLocation differenceLocation)
        {
            return components.Select(c => new MissingComponentDifference(c, differenceLocation));
        }

        public static IEnumerable<Tuple<TR, TR>> JoinOn<T, TKey, TR>(this IEnumerable<T> source,
            IEnumerable<T> dest,
            Func<T, TKey> selector,
            Func<T, TR> result)
        {
            return source.Join(dest, selector, selector, (t1, t2) => new Tuple<TR, TR>(result(t1), result(t2)));
        }

        public static bool IsIn(this Component component, IEnumerable<Component> target)
        {
            return target.Any(c => c.Id == component.Id);
        }

        public static bool IsNotIn(this Component component, IEnumerable<Component> target)
        {
            return target.All(c => c.Id != component.Id);
        }
    }
}