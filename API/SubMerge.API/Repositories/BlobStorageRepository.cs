using Azure.Storage.Blobs;

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

        /// <summary>
        /// Creating containers wiht a public access
        /// since the files will be used directly form UI in version 2
        /// </summary>
        /// <param name="containerNames"></param>
        /// <returns></returns>
        private async Task EnsureContainers(List<string> containerNames)
        {
            // TODO : Remove public access along with version 3 and provide data through an endpoint wiht authorization/authentication
            containerNames.ForEach(containerName =>
                serviceClient.GetBlobContainerClient(containerName).CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob));
        }
    }

}
