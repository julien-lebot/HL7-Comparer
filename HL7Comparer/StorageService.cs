using System;
using System.IO;

namespace HL7Comparer
{
    public class StorageService : IStorageService
    {
        private readonly StorageFolder _applicationDataFolder;
        private readonly StorageFolder _temporaryFolder;

        public StorageService()
        {
            _applicationDataFolder = new StorageFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HL7 Comparer"));
            _temporaryFolder = new StorageFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HL7 Comparer"));

            _applicationDataFolder.CreateIfNotExists();
            _temporaryFolder.CreateIfNotExists();
        }


        public IStorageFolder GetApplicationDataFolder()
        {
            return _applicationDataFolder;
        }

        public IStorageFolder GetTemporaryFolder()
        {
            return _temporaryFolder;
        }
    }
}