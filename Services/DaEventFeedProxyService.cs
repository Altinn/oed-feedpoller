﻿using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Digdir.Oed.FeedPoller.Interfaces;
using Digdir.Oed.FeedPoller.Settings;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digdir.Oed.FeedPoller.Services;
public class DaEventFeedProxyService : IDaEventFeedProxyService
{
    private readonly ILogger<DaEventFeedProxyService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OedSettings _settings;

    public DaEventFeedProxyService(
        ILoggerFactory loggerFactory,
        IOptions<OedSettings> settings, 
        IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<DaEventFeedProxyService>();
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

        if (!Regex.IsMatch(parts[0], _settings.DaProxyHostEndpointMatch))
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

        _logger.LogInformation("Proxying request to {Url} with {Headers}", url, JsonSerializer.Serialize(incomingRequest.Headers));

        if (incomingRequest.Headers.TryGetValues("Authorization", out var authHeaderValues))
        {
            outgoingRequest.Headers.Add("Authorization", authHeaderValues.ToArray());
        }
        
        try
        {
            var incomingResponse = await client.SendAsync(outgoingRequest);
            _logger.LogInformation("Proxying request to {Url} with {BearerToken}", url, outgoingRequest.Headers!.Authorization);
            _logger.LogInformation("Proxying response to {Url} with {BearerToken}", url, incomingResponse.RequestMessage!.Headers!.Authorization);
            var outgoingResponse = incomingRequest.CreateResponse(incomingResponse.StatusCode);
            outgoingResponse.Headers.Add("Content-Type", "application/json");

            await (await incomingResponse.Content.ReadAsStreamAsync()).CopyToAsync(outgoingResponse.Body);

            return outgoingResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request");
            Console.WriteLine(ex);
            throw;
        }
    }
}
