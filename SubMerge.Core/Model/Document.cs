using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubMerge.Core.Model
{
    public class Document
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public int Count { get; set; }
        public string File1 { get; set; }
        public string File2 { get; set; }
        public string MergedFile { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Ready;
        public string Category { get; set; }

        [JsonIgnore]
        public IEnumerable<Record> Records { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public enum DocumentStatus
    {
        Ready
    }
}
