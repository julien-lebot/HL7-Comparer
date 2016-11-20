namespace HL7Comparer
{
    public class RepeatedField
    {
        public int Index { get; }
        public Field ParentField { get; }
        public IIndexedList<int, Component> Components { get; }

        public RepeatedField(Field parentField, int repeatedFieldIndex)
        {
            Index = repeatedFieldIndex;
            ParentField = parentField;
            Components = new IndexedList<int, Component>(c => c.ComponentIdx, componentIndex => new Component(ParentField.ParentSegment, ParentField.Index, componentIndex));
        }
    }
}