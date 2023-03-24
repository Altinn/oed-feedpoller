using System.Net.Http;
using System.Text.Json;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Oed.FeedPoller.Interfaces;

namespace Oed.FeedPoller.Services;
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
    public async Task PostEvent(CloudEvent cloudEvent)
    {
        await Task.CompletedTask;
        _logger.LogInformation("POSTING EVENT: " + JsonSerializer.Serialize(cloudEvent));
    }
}
