using System;
using System.IO;
using System.Threading.Tasks;

namespace MonteCristo.FileService
{
    public interface IFileService
    {
        string GetFullPath(string fileName);

        Task<string> UpsertAsync(Stream inputStream, string fileName, string rootFolder, bool createSubDateFolder = true);

        Task<string> UpsertAsync(string base64String, string fileName, string rootFolder, bool createSubDateFolder = true);

        Task DeleteAsync(string path);
    }
}
