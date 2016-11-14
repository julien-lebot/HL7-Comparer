namespace HL7Comparer.Services
{
    public interface IStorageService
    {
        IStorageFolder GetApplicationDataFolder();
        IStorageFolder GetTemporaryFolder();
    }
}