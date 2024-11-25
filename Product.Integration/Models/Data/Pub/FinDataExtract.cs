using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Product.Integration.Models.Data.Pub
{
    public class FinDataExtract
    {
        [JsonPropertyName("entityId")]
        public string EntityID { get; set; }
        [JsonPropertyName("jobId")]
        public string JobId { get; set; }
        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }
    }
}
