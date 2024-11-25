using System.Text.Json.Serialization;

namespace Product.Integration.Models.Data.Response
{
    public class BBResponse : TResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
    }
}