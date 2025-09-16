using System;
using System.IO;
using System.Threading.Tasks;

namespace Munk.Azure.Storage
{
    public interface IAzureStorageExporter : IAzureStorageBase
    {
        Task<Stream> ExportFileFromAzureStorageAsync(string containerName, Guid fileId);
    }
}
