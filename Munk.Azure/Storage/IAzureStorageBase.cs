using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Munk.Azure.Storage
{
    public interface IAzureStorageBase
    {
        public Task<bool> ExistsFileInAzureStorageAsync(string containerName, Guid fileId);
        public Task<bool> DeleteFileInAzureStorageAsync(string containerName, Guid fileId);
    }
}
