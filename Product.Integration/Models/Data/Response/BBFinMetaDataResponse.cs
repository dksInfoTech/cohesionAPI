using System.Text.Json.Serialization;

namespace Product.Integration.Models.Data.Response
{
    public class BBFinMetaDataResponse : TResponse
    {
        [JsonPropertyName("contains")]
        public List<MetaDataItem> Contains { get; set; }
    }

    public class MetaDataItem
    {
        [JsonPropertyName("cleanName")]
        public string CleanName { get; set; }
        [JsonPropertyName("dlCommercialModelCategory")]
        public string Category { get; set; }
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
        [JsonPropertyName("mnemonic")]
        public string Mnemonic { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

}