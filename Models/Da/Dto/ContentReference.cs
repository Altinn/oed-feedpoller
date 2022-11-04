using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;

public class ContentReference
{
    [JsonPropertyName("@cid")]
    public string Uri { get; set; } = string.Empty;
}