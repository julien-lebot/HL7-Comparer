namespace HL7Comparer
{
    public class ComponentValueDifference : IDifference
    {
        public ComponentValueDifference(Component source, Component target)
        {
            Source = source;
            Target = target;
        }

        public Component Source { get; }
        public Component Target { get; }
        public virtual string AsText()
        {
            if (!string.IsNullOrEmpty(Source.Segment.GetVariableId()))
                return $"{Source.Id} with variable ID {Source.Segment.GetVariableId()} differs: {Source} => {Target}";
            return $"{Source.Id} differs: {Source} => {Target}";
        }
    }
}