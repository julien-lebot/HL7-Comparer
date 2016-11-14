using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public static class MessageExtensions
    {
        public static IEnumerable<string> GetSegmentNames(this Message message)
        {
            return message.Segments.Select(s => s.Name);
        }

        public static Component GetComponentById(this Message message, string id)
        {
            var dashIndex = id.IndexOf('-');
            var dotIndex = id.IndexOf('.');
            if (dashIndex < 3)
            {
                return null;
            }
            if (dotIndex < 4)
            {
                return null;
            }
            var segmentName = id.Substring(0, dashIndex);
            if (string.IsNullOrEmpty(segmentName))
            {
                return null;
            }
            int fieldIndex = 0;
            if (!int.TryParse(id.Substring(dashIndex + 1, dotIndex - dashIndex - 1), out fieldIndex))
            {
                return null;
            }
            int componentIndex = 0;
            if (!int.TryParse(id.Substring(dotIndex + 1), out componentIndex))
            {
                return null;
            }
            var query = from s in message.Segments
                where s.Name == segmentName &&
                      s.Fields.ContainsKey(fieldIndex) &&
                      s.Fields[fieldIndex].Components.ContainsKey(componentIndex)
                select s.Fields[fieldIndex].Components[componentIndex];
            return query.FirstOrDefault();
        }
    }
}