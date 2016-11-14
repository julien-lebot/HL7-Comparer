namespace HL7Comparer
{
    public interface IStorageService
    {
        IStorageFolder GetApplicationDataFolder();
        IStorageFolder GetTemporaryFolder();
    }
}