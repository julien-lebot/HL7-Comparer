using System.Collections.Generic;

namespace HL7Comparer
{
    public interface IMessagesComparer
    {
        IEnumerable<IDifference> Compare(Message source, Message target, IReadOnlyCollection<string> idsToIgnore);
    }
}