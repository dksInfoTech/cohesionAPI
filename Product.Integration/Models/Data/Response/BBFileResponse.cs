using System.Text.Json.Serialization;

namespace Product.Integration.Models.Data.Response
{
    public class BBFileResponse : TResponse
    {
        [JsonPropertyName("contains")]
        public List<ContentItem> Contains { get; set; }
        [JsonPropertyName("view")]
        public View View { get; set; }
    }

    public class ContentItem
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("headers")]
        public Headers Headers { get; set; }
        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Headers
    {
        public string ETag { get; set; }
        public string Digest { get; set; }
        [JsonPropertyName("Content-Type")]
        public string ContentType { get; set; }
        [JsonPropertyName("Last-Modified")]
        public string LastModified { get; set; }
        [JsonPropertyName("Content-Length")]
        public int ContentLength { get; set; }
    }

    public class Metadata
    {
        public string DL_REQUEST_ID { get; set; }
        public string DL_SNAPSHOT_TZ { get; set; }
        public string DL_REQUEST_NAME { get; set; }
        public string DL_REQUEST_TYPE { get; set; }
        public DateTime DL_SNAPSHOT_START_TIME { get; set; }
    }

    public class View
    {
        [JsonPropertyName("next")]
        public string Next { get; set; }
    }
}