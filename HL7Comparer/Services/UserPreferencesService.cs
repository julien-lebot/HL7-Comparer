using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HL7Comparer.Services
{
    public class UserPreferencesService : IUserPreferencesService
    {
        private bool _isDirty;
        private const string FileName = "UserPreferences";
        private readonly IStorageFolder _storageFolder;
        private Dictionary<string, object> _userPreferences = new Dictionary<string, object>();

        public UserPreferencesService(IStorageFolder storageFolder)
        {
            _storageFolder = storageFolder;
        }

        public event EventHandler PreferencesSaved;

        public bool TryGetPreference<T>(string name, ref T backingField)
        {
            if (_userPreferences.ContainsKey(name))
            {
                backingField = (T) _userPreferences[name];
                return true;
            }
            return false;
        }

        public bool TryGetPreference<T>(string name, ICollection<T> result)
        {
            if (_userPreferences.ContainsKey(name))
            {
                var pref = _userPreferences[name];
                if (pref is JArray)
                {
                    var jArray = (JArray) pref;
                    foreach (var element in jArray)
                    {
                        result.Add(element.Value<T>());
                    }
                }
                else
                {
                    var collection = (IEnumerable) pref;
                    foreach (var element in collection)
                    {
                        result.Add((T) element);
                    }
                }

                return true;
            }
            return false;
        }

        public void Set(string name, object value)
        {
            _userPreferences[name] = value;
            _isDirty = true;
        }

        public async Task Save()
        {
            try
            {
                if (!_isDirty)
                {
                    return;
                }
                await
                    _storageFolder.SaveTextAsync(FileName,
                        JsonConvert.SerializeObject(_userPreferences, Formatting.Indented));
                PreferencesSaved?.Invoke(this, EventArgs.Empty);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task Load()
        {
            try
            {
                if (_storageFolder.Exists(FileName))
                {
                    var prefs =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(
                            await _storageFolder.LoadTextAsync(FileName));
                    if (prefs != null)
                    {
                        _userPreferences = prefs;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}