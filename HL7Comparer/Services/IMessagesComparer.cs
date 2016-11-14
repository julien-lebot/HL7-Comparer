using System.Collections.Generic;

namespace HL7Comparer.Services
{
    public interface IMessagesComparer
    {
        IEnumerable<IDifference> Compare(Message source, Message target, IReadOnlyCollection<string> idsToIgnore);
    }
}