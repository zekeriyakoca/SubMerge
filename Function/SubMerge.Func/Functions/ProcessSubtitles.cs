using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SubMerge.Engine;
using SubMerge.Engine.Utils;
using System.Collections.Generic;
using System.Linq;
using Submerge.Engine.Model;
using System.Text;
using System;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http;

namespace SubMerge.Func
{
    public static class ProcessSubtitles
    {
        private static readonly IProcessService processService = new ProcessService();
        static ProcessSubtitles()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [FunctionName("Process")]
        public static async Task<IActionResult> Process(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("ProcessSubtitles.Process function processed a request.");

            bool.TryParse(req.Query["reverse"], out bool reverse);

            var files = req.Form.Files;
            if (files?.Count != 2)
            {
                log.LogWarning("Reqeust received with {0} files", files == default ? 0 : files.Count);
                return new BadRequestObjectResult("Hey! I HAVE SAID TWO FILES!");
            }

            var entries = await processService.GetFirstEntriesAsync(ReadAllLines(files[0]));
            entries = await processService.FillSecondEntriesAsync(ReadAllLines(files[1]), entries.ToList());
            entries = processService.TryFixEntries(entries.ToList());

            log.LogInformation("Files has been processed successfully");
            var fileContent = entries.Select(e => BuildRecord(e, reverse));
            var bytes = Constants.TrEncoding.GetBytes(String.Join(String.Empty, fileContent));

            log.LogInformation("Merged file returns. File Size : {0} KB", (bytes.Count() / 1024));
            return new FileContentResult(bytes, "text/plain");
        }

        private static IEnumerable<string> ReadAllLines(IFormFile formFile)
        {
            using Stream stream = formFile.OpenReadStream();
            using StreamReader reader = new(stream, Constants.TrEncoding);
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static string BuildRecord(Entry e, bool reverse = false)
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
