using oed_feedpoller.Interfaces;

namespace oed_feedpoller.Models;
public class DaEvent
{
    public string EventId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Estate { get; set; } = string.Empty;
    public object? EventData { get; set; }
}
