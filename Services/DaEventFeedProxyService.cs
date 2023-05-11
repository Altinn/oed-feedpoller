using System.Net;
using System.Text.RegularExpressions;
using Digdir.Oed.FeedPoller.Interfaces;
using Digdir.Oed.FeedPoller.Settings;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;

namespace Digdir.Oed.FeedPoller.Services;
public class DaEventFeedProxyService : IDaEventFeedProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _settings;

    public DaEventFeedProxyService(IOptions<ApiSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
    }

    public async Task<HttpResponseData> ProxyRequest(HttpRequestData incomingRequest)
    {
        var client = _httpClientFactory.CreateClient(Constants.DaHttpClient);
        var parts = incomingRequest.Url.AbsolutePath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            var response = incomingRequest.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("No endpoint provided");
            return response;
        }

        if (!Regex.IsMatch(parts[0], _settings.ProxyHostEndpointMatch))
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
