using System.Reflection.PortableExecutable;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Settings;

namespace oed_feedpoller.Services;
public class DaEventFeedService : IDaEventFeedService
{
    private readonly ILogger<DaEventFeedService> _logger;
    private readonly HttpClient _client;
    private readonly DaSettings _settings;

    public DaEventFeedService(
        IOptions<DaSettings> settings,
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory)
    {
        _logger = loggerFactory.CreateLogger<DaEventFeedService>();
        _client = clientFactory.CreateClient("DaHttpClient");
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DaEvent> GetEvents(Cursor cursor)
    {
        await Task.CompletedTask;

        var eventList = new List<DaEvent>
        {
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001") },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002") },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000003") },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000004") },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000005") },
        };

        foreach (var d in eventList)
        {
            _logger.LogInformation($"Yielding DA event with id {d.Id}");
            yield return d;
        }
    }

    public async Task<HttpResponseData> ProxyRequest(HttpRequestData incomingRequest)
    {
        var outgoingRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(_settings.Endpoint + incomingRequest.Url.PathAndQuery));
        outgoingRequest.Headers.Add("Accept", "application/json");

        var incomingResponse = await _client.SendAsync(outgoingRequest);
        var outgoingResponse = incomingRequest.CreateResponse(incomingResponse.StatusCode);
        outgoingResponse.Headers.Add("Content-Type", "application/json");
    
        await (await incomingResponse.Content.ReadAsStreamAsync()).CopyToAsync(outgoingResponse.Body);

        return outgoingResponse;
    }
}
