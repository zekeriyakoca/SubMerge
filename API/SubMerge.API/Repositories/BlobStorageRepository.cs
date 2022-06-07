using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SubMerge.API.Repositories
{
    public class BlobStorageRepository : IBlobStorageRepository, IDataInitializer
    {
        private readonly BlobServiceClient serviceClient;

        public BlobStorageRepository(IConfiguration configuration)
        {
            serviceClient = new BlobServiceClient(configuration["AzureBlogStorage:ConnectionString"]);
        }

        public Task Initialize() =>
            EnsureContainers(new List<string>() { Constants.BlobContainers.FilesToProcess, Constants.BlobContainers.FilesProccessed });


        public async Task<BinaryData> GetBlobToProcessAsync(string blobName)
        {
            return await GetBlob(Constants.BlobContainers.FilesToProcess, blobName);
        }

        public async Task<BinaryData> GetBlobProceessed(string blobName)
        {
            return await GetBlob(Constants.BlobContainers.FilesProccessed, blobName);
        }

        public async Task<bool> UpsertBlog(string containerName, string blobName, Stream stream)
        {
            if (String.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }
            if (String.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            var response = await GetContainerClient(containerName).UploadBlobAsync(blobName, stream);

            return response.Value == null;
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            return serviceClient.GetBlobContainerClient(containerName);
        }

        private async Task<BinaryData> GetBlob(string containerName, string blobName)
        {
            if (String.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }
            if (String.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            var response = await GetContainerClient(containerName).GetBlobClient(blobName).DownloadContentAsync();
            if (response.Value == null)
            {
                return null;
            }
            return response.Value.Content; 
        }

        private async Task EnsureContainers(List<string> containerNames)
        {
            containerNames.ForEach(containerName =>
                serviceClient.GetBlobContainerClient(containerName).CreateIfNotExistsAsync());
        }
    }

}
