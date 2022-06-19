namespace SubMerge.API.Repositories
{
    public interface IBlobStorageRepository
    {
        Task<BinaryData> GetBlobProceessed(string blobName);
        Task<BinaryData> GetBlobToProcessAsync(string blobName);
        Task<bool> UpsertBlog(string containerName, string blobName, Stream stream);
    }
}