using System.Text.Json.Serialization;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Models.Da;

/// <summary>
/// Fullmakt for innsyn og behandling av formue i dødsbo
/// </summary>
public class Formuesfullmakt
{
    [JsonPropertyName("avdoede")]
    public ContentReference Deceased { get; set; } = new();

    [JsonPropertyName("mottaker")]
    public ContentReference Recipient { get; set; } = new();

    [JsonPropertyName("giver")]
    public ContentReference Giver { get; set; } = new();
}