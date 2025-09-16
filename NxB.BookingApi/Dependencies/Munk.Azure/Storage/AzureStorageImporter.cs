using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Munk.Azure.Storage
{
    public class AzureStorageImporter : AzureStorageBase, IAzureStorageImporter
    {
        public const string CONTENT_TYPE_PDF = "application/pdf";

        public AzureStorageImporter(TelemetryClient telemetryClient) : base(telemetryClient)
        {
        }

        public async Task<string> ImportFileToAzureStorageAsync(string containerName, Stream stream, Guid fileId, string contentType)
        {
            contentType ??= CONTENT_TYPE_PDF;
            string fileName;

            try
            {
                containerName = containerName.ToLower();
                var cloudBlobContainer = GetBlobContainer(containerName);
                await cloudBlobContainer.CreateIfNotExistsAsync();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloudBlobContainer.SetPermissionsAsync(permissions);
                fileName = fileId.ToString();

                // Get a reference to the blob address, then upload the file to the blob.
                // Use the value of localFileName for the blob name.
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                cloudBlockBlob.Properties.ContentType = contentType;
                    
                stream.Seek(0, SeekOrigin.Begin);
                await cloudBlockBlob.UploadFromStreamAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch (StorageException ex)
            {
                _telemetryClient.TrackTrace($"error uploading to containerName: {containerName}, fileId: {fileId}, contentType: {contentType}");
                throw;
            }

            var newUrl = BuildBlobUrl(containerName, fileName);
            return newUrl;
        }

        private static string BuildBlobUrl(string storageContainer, string fileName)
        {
            var newUrl = "https://nxbfilestorage.blob.core.windows.net/" + storageContainer + "/" + fileName;
            return newUrl;
        }
    }
}
