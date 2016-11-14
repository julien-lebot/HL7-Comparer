using System.Threading.Tasks;

namespace HL7Comparer
{
    public interface ICacheService
    {
        Task Save(string key, object value);
        Task<TValue> Load<TValue>(string key);
        bool KeyExists(string key);
        Task Delete(string key);
    }
}