using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Munk.Azure.Storage
{
    public class AzureStorageDeleter : AzureStorageBase, IAzureStorageDeleter
    {
        public AzureStorageDeleter(TelemetryClient telemetryClient) : base(telemetryClient)
        {
        }

        public async Task<bool> DeleteFileFromAzureStorageAsync(string containerName, Guid fileId)
        {
            try
            {
                containerName = containerName.ToLower();
                CloudBlobContainer cloudBlobContainer = GetBlobContainer(containerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileId.ToString());

                var result = await cloudBlockBlob.DeleteIfExistsAsync();
                return result;
            }
            catch (StorageException ex)
            {
                _telemetryClient.TrackException(ex);
                Console.WriteLine((string)"Error returned from the AzureStorageDeleter: {0}", (object)ex.Message);
                throw;
            }
        }
    }
}
