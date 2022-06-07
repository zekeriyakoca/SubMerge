
using Newtonsoft.Json;

namespace SubMerge.Core.Model
{
    public class Record
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public RecordStatus Status { get; set; }
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }
        public string Comments { get; set; }
        public string Configuration { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public enum RecordStatus
    {
        Ready
    }
}