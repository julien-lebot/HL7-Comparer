using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HL7Comparer.Services
{
    public interface IUserPreferencesService
    {
        event EventHandler PreferencesSaved;
        bool TryGetPreference<T>(string name, ref T backingField);
        bool TryGetPreference<T>(string name, ICollection<T> result);
        void Set(string name, object value);
        Task Save();
        Task Load();
    }
}