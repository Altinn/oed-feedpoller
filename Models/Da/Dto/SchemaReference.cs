using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;

public class SchemaReference
{
    [JsonPropertyName("$id")]
    public string? Id { get; set; } = string.Empty;

    [JsonPropertyName("$schema")]
    public string Schema { get; set; } = string.Empty;
}