using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ReactiveUI;

namespace HL7Comparer.Services
{
    public interface IHL7Editor : IReactiveObject
    {
        IDocument Document { get; }
        Message HL7Message { get; }
        bool IsModified { get; }

        /// <summary>
        /// Adds a marker for the given HL7 component.
        /// The HL7 component must be located in the parsed document.
        /// </summary>
        /// <param name="component">The component to mark on the document.</param>
        /// <param name="message">The message to associate with the marker.</param>
        void AddHL7ComponentMarker(Component component, string message);
        void ClearMarkers();

        Task LoadFromCache(string key);
        Task SaveInCache(string key);
    }
}