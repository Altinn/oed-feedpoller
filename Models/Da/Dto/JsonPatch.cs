using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;
public class JsonPatch
{
    [JsonPropertyName("op")]
    public JsonPatchOp Op { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = null!;

    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JsonPatchOp
{
    Add,
    Remove,
    Replace,
    Move,
    Copy,
    Test
}
