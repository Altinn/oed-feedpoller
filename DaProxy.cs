using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller
{
    public class DaProxy
    {
        private readonly IDaEventFeedService _daEventFeedService;
        private readonly ILogger _logger;


        public DaProxy(ILoggerFactory loggerFactory, IDaEventFeedService daEventFeedService)
        {
            _daEventFeedService = daEventFeedService;
            _logger = loggerFactory.CreateLogger<DaProxy>();
        }

        [Function("DaProxy")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "{*any}")] HttpRequestData req)
        {

            _logger.LogInformation("Random log \x1B[42mwith green background\x1B[49m message");
            _logger.LogInformation("This is information");
            _logger.LogWarning("This is warning");
            _logger.LogError("This is error");

            return await _daEventFeedService.ProxyRequest(req);
        }
    }
}
