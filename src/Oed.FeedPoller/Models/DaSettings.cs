namespace Oed.FeedPoller.Models;
public class DaSettings
{
    public int MaxItemsPerPoll { get; set; }
    public string HistoryEndpoint { get; set; } = string.Empty;
    public string ProxyHostEndpointMatch { get; set; } = string.Empty;
}
