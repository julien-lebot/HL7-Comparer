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
        private const string FileName = "UserPreferences";
        private Dictionary<string, object> _userPreferences = new Dictionary<string, object>();
        private readonly IStorageFolder _storageFolder;

        public UserPreferencesService(IStorageFolder storageFolder)
        {
            _storageFolder = storageFolder;
        }

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
                        result.Add((T)element);
                    }
                }

                return true;
            }
            return false;
        }

        public void Set(string name, object value)
        {
            _userPreferences[name] = value;
        }

        public async Task Save()
        {
            try
            {
                await _storageFolder.SaveTextAsync(FileName, JsonConvert.SerializeObject(_userPreferences, Formatting.Indented));
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
                    _userPreferences = JsonConvert.DeserializeObject<Dictionary<string, object>>(await _storageFolder.LoadTextAsync(FileName));
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }

        }
    }
}