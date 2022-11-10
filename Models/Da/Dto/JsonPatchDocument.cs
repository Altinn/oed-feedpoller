using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da.Dto;
public class JsonPatchDocument
{
    [JsonPropertyName("patch")]
    public List<JsonPatch> Patch { get; set; } = new();
}

