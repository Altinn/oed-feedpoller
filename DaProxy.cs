// This proxy will not work locally, and its catchall route will interfere with the "test" http endpoint for triggering a event list poll
#if !DEBUG
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller;

public class DaProxy
{
    private readonly IDaEventFeedProxyService _daEventFeedProxyService;

    public DaProxy(IDaEventFeedProxyService daEventFeedProxyService)
    {
        _daEventFeedProxyService = daEventFeedProxyService;
    }

    [Function("DaProxy")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "{*any}")] HttpRequestData req)
    {
        return await _daEventFeedProxyService.ProxyRequest(req);
    }
}
#endif