using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;
public class AltinnEventService : IAltinnEventService
{
    private readonly ILogger<AltinnEventService> _logger;
    private readonly HttpClient _client;

    public AltinnEventService(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory)
    {
        _logger = loggerFactory.CreateLogger<AltinnEventService>();
        _client = clientFactory.CreateClient("EventsHttpClient");
    }

    /// <inheritdoc/>
    public async Task PostEvent(CloudEventRequestModel cloudEvent)
    {
        throw new NotImplementedException();
    }
}
