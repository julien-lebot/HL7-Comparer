using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public static class SegmentExtensions
    {
        public static bool IsObx(this Segment segment)
        {
            return segment.Name == "OBX" || segment.Name == "ZCD";
        }

        public static IEnumerable<Segment> QueryObxSegments(this IEnumerable<Segment> segments)
        {
            return segments.Where(segment => segment.IsObx());
        }

        public static string GetVariableId(this Segment obxSegment)
        {
            if (!obxSegment.IsObx())
            {
                return string.Empty;
            }
            return obxSegment.Fields[3].RepeatedFields[1].Components[1].ToString();
        }
    }
}