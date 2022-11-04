using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;

public class SchemaDefinition
{
    [JsonPropertyName("$id")]
    public string Id { get; set; } = string.Empty;


    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;


    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

}