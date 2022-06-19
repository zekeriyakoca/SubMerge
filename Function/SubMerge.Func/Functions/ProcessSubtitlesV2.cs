using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Submerge.Engine.Model;
using SubMerge.Core.Model;
using SubMerge.Engine;
using SubMerge.Engine.Utils;
using SubMerge.Func.Models;

namespace SubMerge.Func
{
    public static class ProcessSubtitlesV2
    {
        private readonly static IProcessService processService;
        static ProcessSubtitlesV2()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            processService = new ProcessService();
        }

        [FunctionName("ProcessV2")]
        public static async Task Run([QueueTrigger("processrequest", Connection = "AzureQueueConnectionString")] string processReqeust,
            IBinder binder,
            [CosmosDB(
                databaseName: "SubMergeDB",
                containerName: "Record",
                Connection = "CosmosDbConnectionString")] IAsyncCollector<Record> recordItemsOut,
            [CosmosDB(
                databaseName: "SubMergeDB",
                containerName: "Document",
                Connection = "CosmosDbConnectionString")]IAsyncCollector<Document> documentItemsOut,
            ILogger log)
        {
            var model = JsonConvert.DeserializeObject<ProcessRequestDto>(processReqeust);
            if (model == null)
            {
                log.LogError($"Provided queue message is not in correct form to be parsed as {nameof(ProcessRequestDto)}");
                return;
            }

            if (String.IsNullOrWhiteSpace(model.File1) || String.IsNullOrWhiteSpace(model.File1))
            {
                log.LogError($"Two files should be provided! You are missing one or both");
                return;
            }
            try
            {
                using var file1 = await GetFileAsync(binder, model.File1);
                using var file2 = await GetFileAsync(binder, model.File2);
                if (file1 == null || file2 == null)
                {
                    log.LogError($"An error occured while trying to download files to process!");
                    return;
                }

                var linesInFile1 = ReadLines(file1);
                var linesInFile2 = ReadLines(file2);

                var entries = (await processService.GetFirstEntriesAsync(linesInFile1))?.ToList();
                entries = (await processService.FillSecondEntriesAsync(linesInFile2, entries))?.ToList();
                if (entries == null || entries.Count() == 0)
                {
                    throw new Exception("No record extracted from files!");
                }
                entries = processService.TryFixEntries(entries).ToList();

                var linesOfMergedFile = entries.Select(e => BuildLines(e, false));
                var mergedFileName = Guid.NewGuid().ToString();
                await SaveFileAsync(binder, mergedFileName, linesOfMergedFile);

                (var document, var records) = BuildRecords(entries, model.File1, model.File2, mergedFileName);

                await documentItemsOut.AddAsync(document);
                foreach (var chunk in records.Chunk(100))
                {
                    var tasks = chunk.Select(item => recordItemsOut.AddAsync(item)).ToArray();
                    tasks.Append(Task.Delay(1000)); // measure not to exceed free throughput
                    Task.WaitAll(tasks);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error occured while processin following request : {JsonConvert.SerializeObject(processReqeust)}", ex);
                return; // We don't want retry process for the high load process within try block
            }

            log.LogInformation($"C# Queue trigger function processed: {processReqeust}");
        }

        private static IEnumerable<string> ReadLines(Stream file1)
        {
            using (var reader = new StreamReader(file1, Constants.TrEncoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static async Task<Stream> GetFileAsync(IBinder binder, string fileName)
        {
            var fileAttribute =
                new BlobAttribute($"filestoprocess/{fileName}", FileAccess.Read);
            fileAttribute.Connection = "AzureStorageConnectionString";
            var fileStream = await binder.BindAsync<Stream>(fileAttribute);
            return fileStream;
        }

        private static async Task SaveFileAsync(IBinder binder, string fileName, IEnumerable<string> lines)
        {
            var fileAttribute =
                new BlobAttribute($"filestoprocess/{fileName}", FileAccess.Write);
            fileAttribute.Connection = "AzureStorageConnectionString";

            using var stream = await binder.BindAsync<Stream>(fileAttribute);
            using var writter = new StreamWriter(stream, Constants.TrEncoding);
            foreach (var line in lines)
            {
                await writter.WriteLineAsync(line);
            }
        }

        private static (Document, IEnumerable<Record>) BuildRecords(IEnumerable<Entry> entries, string file1, string file2, string mergedFile)
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

        static string BuildLines(Entry e, bool reverse)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(reverse ? e.Text2.Trim() : e.Text1.Trim());
            text.AppendLine("---------------");
            text.AppendLine(reverse ? e.Text1.Trim() : e.Text2.Trim());
            text.AppendLine(String.Empty);
            text.AppendLine("///////////////");
            return text.ToString();
        }
    }
}
