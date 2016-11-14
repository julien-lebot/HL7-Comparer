namespace HL7Comparer
{
    public class MissingComponentDifference : IDifference
    {
        public MissingComponentDifference(Component source, DifferenceLocation differenceLocation)
        {
            DifferenceLocation = differenceLocation;
            Source = source;
        }

        public DifferenceLocation DifferenceLocation { get; }
        public Component Source { get; }
        public virtual string AsText()
        {
            if (!string.IsNullOrEmpty(Source.Segment.GetVariableId()))
                 return $"{Source.Id} with variable ID {Source.Segment.GetVariableId()} ({Source}) is missing in {DifferenceLocation.ToString().ToLower()}";
            return $"{Source.Id} ({Source}) is missing in {DifferenceLocation.ToString().ToLower()}";
        }
    }
}