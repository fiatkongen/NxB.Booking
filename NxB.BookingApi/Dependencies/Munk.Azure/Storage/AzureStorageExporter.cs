using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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

                CloudBlobContainer cloudBlobContainer = GetBlobContainer(containerName);
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileId.ToString());

                long fileByteLength = cloudBlockBlob.StreamWriteSizeInBytes;
                byte[] fileContent = new byte[fileByteLength];

                for (int i = 0; i < fileByteLength; i++)
                {
                    fileContent[i] = 0x20;
                }

                var stream = new MemoryStream();
                await cloudBlockBlob.DownloadToStreamAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch (StorageException ex)
            {
                _telemetryClient.TrackException(ex);
                Console.WriteLine((string)"Error returned from the AzureStorageExporter: {0}", (object)ex.Message);
                throw;
            }
        }
    }
}
