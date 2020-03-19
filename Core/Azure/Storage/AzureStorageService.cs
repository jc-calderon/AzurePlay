using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System.IO;

namespace Core.Azure.Storage
{
    public class AzureStorageService : IAzureStorageService
    {
        public string ConnectionString { get; set; }

        public AzureStorageService()
        {
        }

        public AzureStorageService(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public string UploadFromStream(Stream stream, string containerName, string filename)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var blob = this.GetBlobReference(containerName, filename);
            blob.UploadFromStream(stream);
            return blob.Uri.AbsoluteUri;
        }

        private CloudBlockBlob GetBlobReference(string containerName, string filename)
        {
            var cloudBlobClient = this.GetCloudBlobClient();
            var cloudBlobContainer = this.GetCloudBlobContainer(cloudBlobClient, containerName);
            return cloudBlobContainer.GetBlockBlobReference(filename);
        }

        private CloudBlobClient GetCloudBlobClient()
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(this.ConnectionString);
            return cloudStorageAccount.CreateCloudBlobClient();
        }

        private CloudBlobContainer GetCloudBlobContainer(CloudBlobClient client, string containerName)
        {
            var cloudBlobContainer = client.GetContainerReference(containerName);
            cloudBlobContainer.CreateIfNotExists();

            return cloudBlobContainer;
        }
    }
}