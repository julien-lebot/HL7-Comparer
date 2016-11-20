using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Comparer
{
    public class IndexedList<TIndex, TValue> : List<TValue>, IIndexedList<TIndex, TValue> where TValue : class
    {
        private readonly Func<TValue, TIndex> _indexSelector;
        private readonly Func<TIndex, TValue> _valueFactory;

        public IndexedList(Func<TValue, TIndex> indexSelector, Func<TIndex, TValue> valueFactory)
        {
            _indexSelector = indexSelector;
            _valueFactory = valueFactory;
        }

        public TValue this[TIndex index]
        {
            get
            {
                var value = this.FirstOrDefault(v => _indexSelector(v).Equals(index));
                if (value == null)
                {
                    value = _valueFactory(index);
                    Add(value);
                }
                return value;
            }
        }
    }
}