using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Munk.Azure.Storage
{
    public class AzureStorageBase
    {
        protected readonly TelemetryClient _telemetryClient;

        public AzureStorageBase(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }


        public async Task<bool> ExistsFileInAzureStorageAsync(string containerName, Guid fileId)
        {
            containerName = containerName.ToLower();
            CloudBlobContainer cloudBlobContainer = GetBlobContainer(containerName);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileId.ToString());
            return await cloudBlockBlob.ExistsAsync();
        }

        public async Task<bool> DeleteFileInAzureStorageAsync(string containerName, Guid fileId)
        {
            containerName = containerName.ToLower();
            CloudBlobContainer cloudBlobContainer = GetBlobContainer(containerName);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileId.ToString());
            return await cloudBlockBlob.DeleteIfExistsAsync();
        }

        public CloudBlobContainer GetBlobContainer(string containerName)
        {
            containerName = containerName.ToLower();
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=nxbfilestorage;AccountKey=y8i6+S8AwU2KQPyIWJYxi/oznRkOaSw0AGqU1iUy0OO6yhb8Fx7quPayKeyZszc5XOhblhH3vGI+tdl8juD0Ig==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobContainer cloudBlobContainer = null;

            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called storageconnectionstring.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            return cloudBlobContainer;
        }
    }
}