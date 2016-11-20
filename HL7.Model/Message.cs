using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class Message
    {
        public Dictionary<Separators, char> DefaultSeparator = new Dictionary<Separators, char>
        {
            {Separators.FieldSeparator, '|'},
            {Separators.Component, '^'},
            {Separators.FieldRepeat, '~'},
            {Separators.Escape, '\\'},
            {Separators.SubComponent, '&'}
        };

        private Message(ICollection<Segment> segments = null)
        {
            Segments = segments ?? new List<Segment>();
        }

        public ICollection<Segment> Segments { get; }

        private static Segment Parse(string segmentLine, int lineNumber, Dictionary<Separators, char> defaultSeparators)
        {
            Segment segment;
            string[] allFields;
            var isMsh = segmentLine.StartsWith("MSH");
            if (isMsh)
            {
                var separators = segmentLine.Substring(3, 5);
                defaultSeparators[Separators.FieldSeparator] = separators[0];
                defaultSeparators[Separators.Component] = separators[1];
                defaultSeparators[Separators.FieldRepeat] = separators[2];
                defaultSeparators[Separators.Escape] = separators[3];
                defaultSeparators[Separators.SubComponent] = separators[4];
                allFields = segmentLine.Split(defaultSeparators[Separators.FieldSeparator]);
                segment = new Segment(allFields[0], lineNumber);
                segment.Fields[1].RepeatedFields[1].Components[1].Values.Add(
                    defaultSeparators[Separators.FieldSeparator].ToString());
                segment.Fields[2].RepeatedFields[1].Components[1].Values.Add(segmentLine.Substring(4, 4));
            }
            else
            {
                allFields = segmentLine.Split(defaultSeparators[Separators.FieldSeparator]);
                segment = new Segment(allFields[0], lineNumber);
            }

            var idxField = isMsh ? 2 : 1;
            foreach (var field in allFields.Skip(1))
            {
                var fieldRepeats = field.Split(defaultSeparators[Separators.FieldRepeat]);
                var idxRepeatedField = 1;
                foreach (var fieldRepeat in fieldRepeats)
                {
                    var components = fieldRepeat.Split(defaultSeparators[Separators.Component])
                        .Select(c => c.Trim().Replace("\"\"", "").Trim())
                        .Where(c => c.Length > 0);
                    var idxComponent = 1;
                    foreach (var component in components)
                    {
                        segment.Fields[idxField].RepeatedFields[idxRepeatedField].Components[idxComponent].Values.Add(
                            component);
                        ++idxComponent;
                    }
                    ++idxRepeatedField;
                }
                ++idxField;
            }
            return segment;
        }

        public static Message Parse(string input)
        {
            var mshBeginning = input.IndexOf("MSH", StringComparison.InvariantCultureIgnoreCase);
            // Don't do a simple string split here
            // This 'complicated' algorithm serves the purpose to retain
            // line information from the source text.
            // That way empty lines are accounted for, and can be used to
            // correctly match the segment to its line number which is useful
            // when pinpointing exactly where a component failed in a segment.
            var segmentsLines = new List<string>();
            var index = 0;
            for (var i = mshBeginning; i < input.Length; ++i)
            {
                var ch = input[i];
                if (ch == '\r' || ch == '\n')
                {
                    segmentsLines.Add(input.Substring(index, i - index));
                    if (i + 1 < input.Length && (input[i + 1] == '\r' || input[i + 1] == '\n'))
                    {
                        i++;
                    }
                    index = i + 1;
                }
            }

            // We can clean the input here, but not skip lines, see comment above
            // about maintaining line number.
            var msg = new Message();
            for (var i = 0; i < segmentsLines.Count; ++i)
            {
                var segment = segmentsLines[i];
                var trimmedSegment = segment.Trim();
                if (!string.IsNullOrEmpty(trimmedSegment))
                {
                    msg.Segments.Add(Parse(trimmedSegment, i + 1, msg.DefaultSeparator));
                }
            }
            return msg;
        }
    }
}