using Submerge.Engine.Model;
using SubMerge.API.Dtos;
using SubMerge.API.Repositories;
using SubMerge.API.Utils;
using SubMerge.Core.Model;

namespace SubMerge.API.Services
{
    public interface IProcessService
    {
        Task ProcessFiles(ProcessRequestDto model, CancellationToken token);
    }

    public class ProcessService : IProcessService
    {
        private readonly Engine.IProcessService processService;
        private readonly ICosmosRepository cosmosRepository;
        private readonly IBlobStorageRepository blobStorageRepository;

        public ProcessService(Engine.IProcessService processService, ICosmosRepository cosmosRepository, IBlobStorageRepository blobStorageRepository)
        {
            this.processService = processService;
            this.cosmosRepository = cosmosRepository;
            this.blobStorageRepository = blobStorageRepository;
        }

        public async Task ProcessFiles(ProcessRequestDto model, CancellationToken token)
        {
            await GenerateEntries(model, token);
        }

        private async Task GenerateEntries(ProcessRequestDto model, CancellationToken token)
        {
            var file1 = await blobStorageRepository.GetBlobToProcessAsync(model.File1);
            var file2 = await blobStorageRepository.GetBlobToProcessAsync(model.File1);
            var entries = (await processService.GetFirstEntriesAsync(FileHelper.ReadLines(file1)))?.ToList();
            entries = (await processService.FillSecondEntriesAsync(FileHelper.ReadLines(file2), entries))?.ToList();
            if (entries == null || entries.Count() == 0)
            {
                throw new Exception("No record extracted from files!");
            }
            entries = processService.TryFixEntries(entries).ToList();

            (var document, var records) = BuildRecords(entries, model.File1, model.File2, "***");
            await cosmosRepository.CreateDocumentAsync(document);
            await cosmosRepository.CreateRecordsAsync(records, token);
        }

        private (Document, IEnumerable<Record>) BuildRecords(IEnumerable<Entry> entries, string file1, string file2, string mergedFile)
        {
            var documentId = Guid.NewGuid().ToString();
            var records = entries.Select(e =>
            {
                return new Record()
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    Status = RecordStatus.Ready,
                    Text1 = e.Text1,
                    Text2 = e.Text2
                };
            });
            var document = new Document()
            {
                Id = documentId,
                Count = records.Count(),
                File1 = file1,
                File2 = file2,
                MergedFile = mergedFile,
                Status = DocumentStatus.Ready,
                Category = "test-data"
            };

            return (document, records);
        }
    }
}
