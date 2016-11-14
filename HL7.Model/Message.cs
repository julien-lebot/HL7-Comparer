using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Message
    {
        public ICollection<Segment> Segments { get; }
        private Message(ICollection<Segment> segments = null)
        {
            Segments = segments ?? new List<Segment>();
        }

        private static readonly Dictionary<Separators, char> DefaultSeparator = new Dictionary<Separators, char>
        {
            {Separators.Component, '^'},
            {Separators.FieldRepeat, '~'},
            {Separators.Escape, '\\'},
            {Separators.SubComponent, '&'}
        };

        private static Segment Parse(string segmentLine, int lineNumber, Dictionary<Separators, char> defaultSeparators)
        {
            Dictionary<int, Field> fieldsDictionary = new Dictionary<int, Field>();
            var allFields = segmentLine.Split(Constants.FieldSeparator);
            var segment = new Segment(allFields[0], lineNumber);
            var isMsh = segment.Name == "MSH";
            var fields = allFields
                .Skip(1)
                .SelectMany((s, idxField) =>
                {
                    return s.Split(new[] { defaultSeparators[Separators.FieldRepeat] })
                        .Select((r, idxRepeatedField) =>
                        {
                            return new
                            {
                                FieldIndex = idxField + 1,
                                ComponentIndex = idxRepeatedField + 1,
                                Values = r.Split(new[] { defaultSeparators[Separators.Component] })
                                    .Select(c => c.Trim().Replace("\"\"", "").Trim())
                                    .Where(c => c.Length > 0)
                                    .Select((c, idxComponent) => new { Index = idxComponent, Value = c }).ToList()
                            };
                        }).ToList();
                }).ToList();

            foreach (var parsedField in fields)
            {
                foreach (var parsedValue in parsedField.Values)
                {
                    if (!fieldsDictionary.ContainsKey(parsedField.FieldIndex))
                    {
                        fieldsDictionary[parsedField.FieldIndex] = new Field();
                    }
                    if (!fieldsDictionary[parsedField.FieldIndex].Components.ContainsKey(parsedField.ComponentIndex))
                    {
                        fieldsDictionary[parsedField.FieldIndex].Components[parsedField.ComponentIndex] =
                            new Component(segment, parsedField.FieldIndex, parsedField.ComponentIndex);
                    }
                    fieldsDictionary[parsedField.FieldIndex].Components[parsedField.ComponentIndex].Values.Add(parsedValue.Value);
                }
            }
            segment.Fields = fieldsDictionary;
            return segment;
        }

        public static Message Parse(string input)
        {
            // Don't do a simple string split here
            // This 'complicated' algorithm serves the purpose to retain
            // line information from the source text.
            // That way empty lines are accounted for, and can be used to
            // correctly match the segment to its line number which is useful
            // when pinpointing exactly where a component failed in a segment.
            var segments = new List<string>();
            int index = 0;
            for (int i = 0; i < input.Length; ++i)
            {
                char ch = input[i];
                if (ch == '\r' || ch == '\n')
                {
                    segments.Add(input.Substring(index, i - index));
                    if (i + 1 < input.Length && (input[i + 1] == '\r' || input[i + 1] == '\n'))
                    {
                        i++;
                    }
                    index = i + 1;
                }
            }

            var msh = segments.FirstOrDefault(s => s.StartsWith("MSH"));
            if (msh != null)
            {
                var separators = msh.Substring(3, 4);
                DefaultSeparator[Separators.Component] = separators[0];
                DefaultSeparator[Separators.FieldRepeat] = separators[1];
                DefaultSeparator[Separators.Escape] = separators[2];
                DefaultSeparator[Separators.SubComponent] = separators[3];
            }

            // We can clean the input here, but not skip lines, see comment above
            // about maintaining line number.
            var msg = new Message();
            for (var i = 0; i < segments.Count; ++i)
            {
                var segment = segments[i];
                var trimmedSegment = segment.Trim();
                if (!string.IsNullOrEmpty(trimmedSegment) && trimmedSegment.IndexOf(Constants.FieldSeparator) > -1)
                {
                    msg.Segments.Add(Parse(trimmedSegment, i + 1, DefaultSeparator));
                }
            }
            return msg;
        }
    }
}