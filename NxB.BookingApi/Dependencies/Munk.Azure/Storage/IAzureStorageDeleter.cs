using System;
using System.Threading.Tasks;

namespace Munk.Azure.Storage
{
    public interface IAzureStorageDeleter
    {
        Task<bool> DeleteFileFromAzureStorageAsync(string containerName, Guid fileId);
    }
}
