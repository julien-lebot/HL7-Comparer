namespace HL7Comparer
{
    public class Field
    {
        public int Index { get; }
        public Segment ParentSegment { get; }
        public IIndexedList<int, RepeatedField> RepeatedFields { get; }

        public Field(Segment parentSegment, int fieldIndex)
        {
            Index = fieldIndex;
            ParentSegment = parentSegment;
            RepeatedFields = new IndexedList<int, RepeatedField>(rf => rf.Index, repeatedFieldsIndex => new RepeatedField(this, repeatedFieldsIndex));
        }
    }
}