namespace HL7Comparer
{
    public class MissingSegmentDifference : IDifference
    {
        public MissingSegmentDifference(Segment missingSegment, DifferenceLocation missingSegmentDifferenceLocation)
        {
            MissingSegment = missingSegment;
            MissingSegmentDifferenceLocation = missingSegmentDifferenceLocation;
        }

        public Segment MissingSegment { get; }
        public DifferenceLocation MissingSegmentDifferenceLocation { get; }

        public virtual string AsText()
        {
            if (!string.IsNullOrEmpty(MissingSegment.GetVariableId()))
                return $"{MissingSegment.Name} with variable ID {MissingSegment.GetVariableId()} is missing in {MissingSegmentDifferenceLocation.ToString().ToLower()}";
            return $"{MissingSegment.Name} is missing in {MissingSegmentDifferenceLocation.ToString().ToLower()}";
        }
    }
}