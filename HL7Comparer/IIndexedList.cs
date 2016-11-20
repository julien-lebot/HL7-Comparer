using System.Collections;
using System.Collections.Generic;

namespace HL7Comparer
{
    /// <summary>
    /// Represents a collection of objects that can be individually accessed by index.
    /// The items are created on demand when they are accessed.
    /// </summary>
    /// <typeparam name="TIndex">The type of indices in the list.</typeparam>
    /// <typeparam name="TValue">The type of elements in the list.</typeparam>
    public interface IIndexedList<in TIndex, out TValue> : IEnumerable<TValue>, IEnumerable
        where TValue : class
    {
        TValue this[TIndex index] { get; }
    }
}