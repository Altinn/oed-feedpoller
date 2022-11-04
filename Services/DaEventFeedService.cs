using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oed_feedpoller.Exceptions;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Models.Da.Dto;
using oed_feedpoller.Settings;

namespace oed_feedpoller.Services;
public class DaEventFeedService : IDaEventFeedService
{
    private readonly IHydratorFactory _hydratorFactory;
    private readonly IDaApiClient _daApiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DaEventFeedService> _logger;
    private readonly DaSettings _settings;

    public DaEventFeedService(
        IOptions<DaSettings> settings,
        ILoggerFactory loggerFactory,
        IHydratorFactory hydratorFactory,
        IDaApiClient daApiClient,
        IHttpClientFactory httpClientFactory)
    {
        _hydratorFactory = hydratorFactory;
        _daApiClient = daApiClient;
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<DaEventFeedService>();
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DaEvent> GetEvents(Cursor cursor)
    {
        var pollUri = new Uri(_settings.HistoryEndpoint + "?offset=" + cursor.Value + "&top=" +
                              _settings.MaxItemsPerPoll);

        _logger.LogInformation($"Sending request to {pollUri}");

        var request = new HttpRequestMessage(HttpMethod.Get, pollUri);
        var result = await _daApiClient.SendAsync(request);

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogInformation("Got non-successful response code from DA: {code} {body}", result.StatusCode.ToString(), await result.Content.ReadAsStringAsync());
            throw new DaFeedException($"Got non-successful response code from DA: {result.StatusCode.ToString()}");
        }

        var feedList = (await result.Content.ReadFromJsonAsync<List<FeedEntry>>())!;

        foreach (var feedEntry in feedList)
        {
            var daEvent = await GetHydratedEventFromFeedEntry(feedEntry);
            if (daEvent != null) yield return daEvent;
        }
    }

    private async Task<DaEvent?> GetHydratedEventFromFeedEntry(FeedEntry feedEntry)
    {
        var eventId = GetIdFromFeedEntry(feedEntry);
        try
        {
            var eventJson = await _daApiClient.GetCachedAsync(feedEntry.FeedResult.Uri);
            var schemaReference = JsonSerializer.Deserialize<SchemaReference>(eventJson)!;
            
            if (!string.IsNullOrEmpty(schemaReference.Id))
            {
                _logger.LogWarning("Feed entry with id {eventId} points to schema {schemaId}, skipping", eventId, schemaReference.Id);
                return null;
            }
            
            var schemaDefinition = await _daApiClient.GetCachedAsync<SchemaDefinition>(schemaReference.Schema);

            var daEventHydrator = _hydratorFactory.GetHydrator(schemaDefinition);
            if (daEventHydrator != null)
            {
                var daEvent = await daEventHydrator.GetHydratedEvent(eventJson);
                daEvent.EventId = eventId;
                daEvent.Timestamp = DateTimeOffset.FromUnixTimeSeconds(feedEntry.Timestamp);
                return daEvent;
            }

            _logger.LogWarning("Feed entry with id {eventId} is referring to unknown schema id: {schemaId}", eventId, schemaDefinition.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("{exception}: Failed fetching/deserializing {eventUri}: {errorMessage}", ex.GetType().Name, feedEntry.FeedResult.Uri, ex.Message);
            return null;
        }
    }

    private string GetIdFromFeedEntry(FeedEntry feedEntry)
    {
        return feedEntry.FeedResult.Uri.Split('/')[^2];
    }

    public async Task<HttpResponseData> ProxyRequest(HttpRequestData incomingRequest)
    {
        var client = _httpClientFactory.CreateClient(Constants.DaHttpClient);
        var parts = incomingRequest.Url.AbsolutePath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (!Regex.IsMatch(parts[0], _settings.HostEndpointMatch))
        {
            var response = incomingRequest.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Invalid endpoint");
            return response;
        }

        var url = "https:/" + incomingRequest.Url.AbsolutePath;
        var query = incomingRequest.Url.Query;
        var codeParamMatch = Regex.Match(incomingRequest.Url.Query, @"([\?&]code=[^&]*)");
        if (codeParamMatch.Success)
        {
            query = query.Replace(codeParamMatch.Groups[1].Value, string.Empty);
            if (!string.IsNullOrEmpty(query) && query[0] == '&')
            {
                query = '?' + query[1..];
            }
        }
        url += query;

        var outgoingRequest = new HttpRequestMessage(HttpMethod.Get, url);
        outgoingRequest.Headers.Add("Accept", "application/json");

        var incomingResponse = await client.SendAsync(outgoingRequest);
        var outgoingResponse = incomingRequest.CreateResponse(incomingResponse.StatusCode);
        outgoingResponse.Headers.Add("Content-Type", "application/json");
    
        await (await incomingResponse.Content.ReadAsStreamAsync()).CopyToAsync(outgoingResponse.Body);

        return outgoingResponse;
    }
}
