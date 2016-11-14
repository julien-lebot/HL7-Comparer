using System.Collections.Generic;
using System.Threading.Tasks;

namespace HL7Comparer
{
    public interface IUserPreferencesService
    {
        bool TryGetPreference<T>(string name, ref T backingField);
        bool TryGetPreference<T>(string name, ICollection<T> result);
        void Set(string name, object value);
        Task Save();
        Task Load();
    }
}