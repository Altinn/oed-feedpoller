using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;
public class DaEventFeedService : IDaEventFeedService
{
    private readonly ILogger<DaEventFeedService> _logger;
    private readonly HttpClient _client;

    public DaEventFeedService(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory)
    {
        _logger = loggerFactory.CreateLogger<DaEventFeedService>();
        _client = clientFactory.CreateClient("EventsHttpClient");
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<DaEvent> GetEvents(Cursor cursor)
    {
        throw new NotImplementedException();
    }
}
