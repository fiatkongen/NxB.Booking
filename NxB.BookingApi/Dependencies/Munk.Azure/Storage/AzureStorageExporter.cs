using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Azure.Storage.Blobs;
using Azure;

namespace Munk.Azure.Storage
{
    public class AzureStorageExporter : AzureStorageBase, IAzureStorageExporter
    {
        public AzureStorageExporter(TelemetryClient telemetryClient) : base(telemetryClient)
        {
        }

        public async Task<Stream> ExportFileFromAzureStorageAsync(string containerName, Guid fileId)
        {
            try
            {
                containerName = containerName.ToLower();

                BlobContainerClient blobContainerClient = GetBlobContainer(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileId.ToString());

                var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch (RequestFailedException ex)
            {
                _telemetryClient.TrackException(ex);
                Console.WriteLine((string)"Error returned from the AzureStorageExporter: {0}", (object)ex.Message);
                throw;
            }
        }
    }
}
