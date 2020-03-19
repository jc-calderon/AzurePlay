using System.IO;

namespace Core.Azure.Storage
{
    public interface IAzureStorageService
    {
        string ConnectionString { get; set; }

        string UploadFromStream(Stream stream, string containerName, string filename);
    }
}