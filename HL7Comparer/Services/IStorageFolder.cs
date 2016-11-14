using System.IO;
using System.Threading.Tasks;

namespace HL7Comparer.Services
{
    public interface IStorageFolder
    {
        /// <summary>
        /// Save a given string in the file.
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        /// <param name="text">The string to save.</param>
        /// <returns>A task that will complete once the operation is done.</returns>
        Task SaveTextAsync(string fileName, string text);

        /// <summary>
        /// Reads the file content in a string.
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        /// <returns>A task that when awaited returns the content of the file.</returns>
        Task<string> LoadTextAsync(string fileName);

        /// <summary>
        /// Get a stream to write to the file.
        /// </summary>
        /// <remarks>Dispose the stream after use.</remarks>
        /// <param name="fileName">The name of the file to use.</param>
        /// <returns>A write-only stream.</returns>
        Stream GetWriteStream(string fileName);

        /// <summary>
        /// Get a stream to read from the file.
        /// </summary>
        /// <remarks>Dispose the stream after use.</remarks>
        /// <param name="fileName">The name of the file to use.</param>
        /// <returns>A write-only stream.</returns>
        Stream GetReadStream(string fileName);

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <param name="fileName">The name of the file to use.</param>
        Task DeleteAsync(string fileName);
        
        /// <summary>
        /// Checks whether a given file exists in the folder.
        /// </summary>
        /// <param name="fileName">The name of the file to check.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        bool Exists(string fileName);
    }
}