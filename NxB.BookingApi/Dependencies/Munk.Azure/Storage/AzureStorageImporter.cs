using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;

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
                var blobContainerClient = GetBlobContainer(containerName);
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                fileName = fileId.ToString();

                // Get a reference to the blob address, then upload the file to the blob.
                // Use the value of localFileName for the blob name.
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                };

                stream.Seek(0, SeekOrigin.Begin);
                await blobClient.UploadAsync(stream, blobUploadOptions);
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch (RequestFailedException ex)
            {
                _telemetryClient.TrackTrace($"error uploading to containerName: {containerName}, fileId: {fileId}, contentType: {contentType}");
                _telemetryClient.TrackException(ex);
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
