using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;

public class FeedEntry
{
    [JsonPropertyName("change")]
    public ContentReference FeedChange { get; set; } = new();

    [JsonPropertyName("result")]
    public ContentReference FeedResult { get; set; } = new();

    [JsonPropertyName("ts")]
    public int Timestamp { get; set; }
}