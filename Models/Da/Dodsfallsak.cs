using System.Text.Json.Serialization;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Models.Da;

/// <summary>
/// Sak for dødsfallsbehandling
/// </summary>
public class Dodsfallsak
{

    [JsonPropertyName("parter")]
    public List<Party> Parties { get; set; } = new();
}

public class Party
{
    [JsonPropertyName("part")]
    public ContentReference PartyCid { get; set; } = new();

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}