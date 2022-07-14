using Microsoft.Azure.Cosmos;
using SubMerge.Core.Model;

namespace SubMerge.API.Repositories
{

    public class CosmosCommonRepository : ICosmosRepository
    {
        private readonly CosmosClient client;
        private readonly Container documentContainer;
        private readonly Container recordContainer;

        public CosmosCommonRepository(CosmosClient client, IConfiguration configuration)
        {
            this.client = client;
            documentContainer = client.GetContainer(configuration["CosmosDb:Database"], Constants.CosmosContainers.Document);
            recordContainer = client.GetContainer(configuration["CosmosDb:Database"], Constants.CosmosContainers.Record);
        }

        public async Task<Document> CreateDocumentAsync(Document document)
        {
            var response = await documentContainer.UpsertItemAsync(document);
            return response.Resource;
        }

        public async Task CreateRecordsAsync(IEnumerable<Record> records, CancellationToken token)
        {
            var parallelOptions = new ParallelOptions() { CancellationToken = token, MaxDegreeOfParallelism = 5 };
            await Parallel.ForEachAsync(records, parallelOptions,
                async (record, token) => await recordContainer.UpsertItemAsync(record, cancellationToken: token));
        }

    }

}
