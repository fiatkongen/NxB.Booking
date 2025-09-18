using System;
using System.IO;
using System.Threading.Tasks;

namespace Munk.Azure.Storage
{
    public interface IAzureStorageImporter : IAzureStorageBase
    {
        Task<string> ImportFileToAzureStorageAsync(string containerName, Stream stream, Guid fileId, string contentType = null);
    }
}