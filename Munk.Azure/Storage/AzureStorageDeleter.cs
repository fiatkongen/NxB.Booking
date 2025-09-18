using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Azure.Storage.Blobs;
using Azure;

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
                BlobContainerClient blobContainerClient = GetBlobContainer(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());

                var response = await blobClient.DeleteIfExistsAsync();
                var result = response.Value;
                return result;
            }
            catch (RequestFailedException ex)
            {
                _telemetryClient.TrackException(ex);
                Console.WriteLine((string)"Error returned from the AzureStorageDeleter: {0}", (object)ex.Message);
                throw;
            }
        }
    }
}
