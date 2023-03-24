using System.Net;
using Domstol.Hendelser.ApiClient;
using CloudNative.CloudEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Oed.FeedPoller.Interfaces;
using System.Text.Json;

namespace Oed.FeedPoller.AzFunc;

public class FeedPollerFunction
{
    private const string DaFeedCursorName = "DaFeedCursorDateTime";

    private readonly IAltinnEventService _altinnEventService;
    private readonly ICursorService _cursorService;
    private readonly ILogger _logger;
    private readonly IDaEventFeedService _eventFeedService;

    public FeedPollerFunction(
        ILoggerFactory loggerFactory,
        IAltinnEventService altinnEventService,
        ICursorService cursorService,
        IDaEventFeedService eventFeedService)
    {
        _logger = loggerFactory.CreateLogger<FeedPollerFunction>();
        _altinnEventService = altinnEventService;
        _cursorService = cursorService;
        _eventFeedService = eventFeedService;
    }

#if !DEBUG
    [Function(nameof(FeedPoller))]
    public async Task RunAsync([TimerTrigger("*/5 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogDebug($"DA feed import executed at: {DateTime.Now}");

        if (timerInfo.IsPastDue)
        {
            _logger.LogWarning("DA feed import was not run on schedule");
        }

        if (!Helpers.ShouldRunUpdate())
        {
            _logger.LogDebug("Skipping update outside of busy hours");
            return;
        }

        await PerformFeedPollAndUpdate();

        _logger.LogInformation($"Next timer schedule at: {timerInfo.ScheduleStatus?.Next}");
    }
#else
    public List<DaEvent> DaEvents { get; set; } = new();
    public List<CloudEvent> CloudEvents { get; set; } = new();

    [Function("typed")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        await PerformFeedPollAndUpdate();
        var response = req.CreateResponse(HttpStatusCode.OK);
        if (req.Url.Query.Contains("cloudevent"))
        {
            await response.WriteAsJsonAsync(CloudEvents);
        }
        else
        {
            await response.WriteAsJsonAsync(DaEvents);
        }

        return response;
    }
#endif

    [Function("test")]
    public async Task<HttpResponseData> TestAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Processing incoming test data...");

        string requestBody = String.Empty;
        using (StreamReader streamReader = new StreamReader(req.Body))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }
        DaEventFeedTestModel testData = JsonSerializer.Deserialize<DaEventFeedTestModel>(requestBody);

        if (testData != null)
        {

            // flatten event feed structure
            var daEvents = testData.DaEventList.SelectMany(list => list).Select(list2 => list2).ToList();

            var eventsList = _eventFeedService.ConvertEventFeed(daEvents, testData.DaCaseList);

            foreach (CloudEvent cloudEvent in eventsList)
            {
                await _altinnEventService.PostEvent(cloudEvent);

                //await _cursorService.UpdateCursor(new Cursor { Name = DaFeedCursorName, Value = daEvent.EventId });
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            if (req.Url.Query.Contains("cloudevent"))
            {
                await response.WriteAsJsonAsync(CloudEvents);
            }
            else
            {
                await response.WriteAsJsonAsync(DaEvents);
            }
            return response;
        }
        else
        {
            _logger.LogError("Unable to deserialize request body.");
        }
    }

    private async Task PerformFeedPollAndUpdate()
    {
        var cursor = await _cursorService.GetCursor(DaFeedCursorName);

        var eventsList = await _eventFeedService.GetEvents(cursor);
        foreach (CloudEvent cloudEvent in eventsList)
        {
            await _altinnEventService.PostEvent(cloudEvent);

            //await _cursorService.UpdateCursor(new Cursor { Name = DaFeedCursorName, Value = daEvent.EventId });
        }
    }
}