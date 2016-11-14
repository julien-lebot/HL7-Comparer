using System.IO;
using System.Threading.Tasks;

namespace HL7Comparer.Services
{
    public class StorageFolder : IStorageFolder
    {
        private readonly string _path;
        private static readonly char[] InvalidChars;

        static StorageFolder()
        {
            InvalidChars = Path.GetInvalidFileNameChars();
        }

        private string GetFilePath(string fileName)
        {
            foreach (var c in InvalidChars)
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }
            return System.IO.Path.Combine(_path, fileName);
        }

        public StorageFolder(string path)
        {
            _path = path;
        }

        public void CreateIfNotExists()
        {
            Directory.CreateDirectory(_path);
        }

        /// <summary>
        /// Save a given string in the file.
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        /// <param name="text">The string to save.</param>
        /// <returns>A task that will complete once the operation is done.</returns>
        public async Task SaveTextAsync(string fileName, string text)
        {
            using (var sw = File.CreateText(GetFilePath(fileName)))
            {
                await sw.WriteAsync(text);
            }
        }

        /// <summary>
        /// Reads the file content in a string.
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        /// <returns>A task that when awaited returns the content of the file.</returns>
        public async Task<string> LoadTextAsync(string fileName)
        {
            using (var sr = File.OpenText(GetFilePath(fileName)))
            {
                return await sr.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Get a stream to write to the file.
        /// </summary>
        /// <remarks>Dispose the stream after use.</remarks>
        /// <returns>A write-only stream.</returns>
        public Stream GetWriteStream(string fileName)
        {
            return File.OpenWrite(GetFilePath(fileName));
        }

        /// <summary>
        /// Get a stream to read from the file.
        /// </summary>
        /// <remarks>Dispose the stream after use.</remarks>
        /// <returns>A write-only stream.</returns>
        public Stream GetReadStream(string fileName)
        {
            return File.OpenRead(GetFilePath(fileName));
        }

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        public Task DeleteAsync(string fileName)
        {
            return Task.Run(() =>
            {
                File.Delete(GetFilePath(fileName));
            });
        }

        /// <summary>
        /// Checks whether a given file exists in the folder.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        public bool Exists(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
        }
    }
}