using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HL7Comparer.Services
{
    public class FileCacheService : ICacheService
    {
        private readonly IStorageFolder _storageFolder;

        public FileCacheService(IStorageFolder storageFolder)
        {
            _storageFolder = storageFolder;
        }

        public async Task Save(string key, object value)
        {
            await _storageFolder.SaveTextAsync(key, JsonConvert.SerializeObject(value));
        }

        public async Task<TValue> Load<TValue>(string key)
        {
            return JsonConvert.DeserializeObject<TValue>(await _storageFolder.LoadTextAsync(key));
        }

        public bool KeyExists(string key)
        {
            return _storageFolder.Exists(key);
        }

        public async Task Delete(string key)
        {
            await _storageFolder.DeleteAsync(key);
        }
    }
}