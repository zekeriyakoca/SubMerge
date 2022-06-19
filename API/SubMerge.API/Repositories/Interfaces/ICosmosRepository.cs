using SubMerge.Core.Model;

namespace SubMerge.API.Repositories
{
    public interface ICosmosRepository
    {
        Task<Document> CreateDocumentAsync(Document model);
        Task CreateRecordsAsync(IEnumerable<Record> records, CancellationToken token);
    }
}
