
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Digdir.Oed.FeedPoller;

public class CloudEventsController
{
    private readonly ILogger<CloudEventsController> _logger;

    public CloudEventsController(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CloudEventsController>();
    }

    [Function("CloudEvents")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "cloudevents")] HttpRequestData req)
    {
        _logger.LogWarning("Cloud event received on webhook endpoint");
        return req.CreateResponse(System.Net.HttpStatusCode.OK);
    }
}