using System;
using System.Collections.Generic;
using System.Linq;
using HL7Comparer.Services;

namespace HL7Comparer
{
    public class MessagesComparer : IMessagesComparer
    {
        public IEnumerable<IDifference> Compare(Message source, Message target, IReadOnlyCollection<string> idsToIgnore)
        {
            var differences = new List<IDifference>();
            {
                var segments =
                    new Tuple<IEnumerable<Segment>, IEnumerable<Segment>>(
                        source.Segments.Where(s => !s.IsObx()).ToList(), target.Segments.Where(s => !s.IsObx()).ToList());

                differences.AddRange(GetMissingSegments(segments, CompareUsing.SegmentName));
                var components = segments.Transform((src, dest) => src.JoinOn(dest, s => s.Name, s => s.Components.Where(c => !string.IsNullOrEmpty(c.ToString()) && !idsToIgnore.Contains(c.Id)))).ToList();
                differences.AddRange(GetMissingComponents(components));
                differences.AddRange(GetComponentsDifferences(components));
            }

            {
                var obxSegments =
                    new Tuple<IEnumerable<Segment>, IEnumerable<Segment>>(
                        source.Segments.Where(s => s.IsObx()).ToList(), target.Segments.Where(s => s.IsObx()).ToList());

                differences.AddRange(GetMissingSegments(obxSegments, CompareUsing.ObxSegmentComparer));
                var obxComponents = obxSegments.Transform((src, dest) => src.JoinOn(dest, s => s.GetVariableId(), s => s.Components.Where(c => !string.IsNullOrEmpty(c.ToString()) && !idsToIgnore.Contains(c.Id)))).ToList();
                differences.AddRange(GetMissingComponents(obxComponents));
                differences.AddRange(GetComponentsDifferences(obxComponents));
            }
            return differences;
        }

        private static IEnumerable<IDifference> GetMissingSegments(Tuple<IEnumerable<Segment>, IEnumerable<Segment>> obxSegments, IEqualityComparer<Segment> segmentEqualityComparer)
        {
            return
                Enumerable.Union(
                    obxSegments.Transform(
                        (src, dest) =>
                            src.Except(dest, segmentEqualityComparer)
                                .AsMissingSegment(DifferenceLocation.Target))
                    ,
                    obxSegments.Transform(
                        (src, dest) =>
                            dest.Except(src, segmentEqualityComparer)
                                .AsMissingSegment(DifferenceLocation.Source))
                    );
        }

        private static IEnumerable<IDifference> GetComponentsDifferences(List<Tuple<IEnumerable<Component>, IEnumerable<Component>>> components)
        {
            return components.Transform((src, dest) =>
            {
                var destList = dest.ToList();
                return src.Except(destList, CompareUsing.ComponentValue)
                          .Where(c => c.IsIn(destList))
                          .Select(c => new ComponentValueDifference(c, destList.First(x => x.Id == c.Id)));
            });
        }

        private static IEnumerable<IDifference> GetMissingComponents(List<Tuple<IEnumerable<Component>, IEnumerable<Component>>> components)
        {
            return
                Enumerable.Union(
                    components.Transform(
                        (src, dest) => src.Where(c => c.IsNotIn(dest)).AsMissingComponent(DifferenceLocation.Target))
                    ,
                    components.Transform(
                        (src, dest) => dest.Where(c => c.IsNotIn(src)).AsMissingComponent(DifferenceLocation.Source))
                    );
        }
    }
}