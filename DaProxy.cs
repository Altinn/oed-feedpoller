using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller;

public class DaProxy
{
    private readonly IDaEventFeedService _daEventFeedService;

    public DaProxy(IDaEventFeedService daEventFeedService)
    {
        _daEventFeedService = daEventFeedService;
    }

    [Function("DaProxy")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "{*any}")] HttpRequestData req)
    {
        return await _daEventFeedService.ProxyRequest(req);
    }
}