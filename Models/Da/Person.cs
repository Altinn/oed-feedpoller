using System.Text.Json.Serialization;

namespace oed_feedpoller.Models.Da;
public class Person
{
    [JsonPropertyName("nin")]
    public string Ssn { get; set; } = string.Empty;
}
