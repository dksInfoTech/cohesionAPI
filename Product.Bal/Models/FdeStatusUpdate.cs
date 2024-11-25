using System.Text.Json.Serialization;

namespace Product.Bal.Models;

public class FdeStatusUpdate
{
    [JsonPropertyName("entityId")]
    public required string EntityId {  get; set; }

    [JsonPropertyName("jobId")]
    public required string JobId {  get; set; }

    [JsonPropertyName("stage")]
    public required string Stage {  get; set; }

    [JsonPropertyName("status")]
    public required string Status {  get; set; }

    [JsonPropertyName("content")]
    public List<FinStatement>? Content { get; set; }

    [JsonPropertyName("message")]
    public string? message {  get; set; }
}

public class FinStatement
{
    [JsonPropertyName("finYear")]
    public required string FinYear { get; set; }

    [JsonPropertyName("content")]
    public List<FinData>? Content { get; set; }
}

public class FinData
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }
}

