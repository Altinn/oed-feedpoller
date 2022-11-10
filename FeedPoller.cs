using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using oed_feedpoller.Exceptions;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Settings;

namespace oed_feedpoller;

public class FeedPoller
{
    private const string DaFeedCursorName = "DaFeedCursor";

    private readonly IDaEventFeedService _daEventFeedService;
    private readonly IAltinnEventService _altinnEventService;
    private readonly ICursorService _cursorService;
    private readonly IEventMapperService _eventMapperService;
    private readonly ILogger _logger;

    public FeedPoller(
        ILoggerFactory loggerFactory,
        IDaEventFeedService daEventFeedService,
        IAltinnEventService altinnEventService,
        ICursorService cursorService,
        IEventMapperService eventMapperService)
    {
        _logger = loggerFactory.CreateLogger<FeedPoller>();
        _daEventFeedService = daEventFeedService;
        _altinnEventService = altinnEventService;
        _cursorService = cursorService;
        _eventMapperService = eventMapperService;
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

    [Function("test")]
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

    private async Task PerformFeedPollAndUpdate()
    {
        var cursor = await _cursorService.GetCursor(DaFeedCursorName);
        await foreach (DaEvent daEvent in _daEventFeedService.GetEvents(cursor))
        {
#if DEBUG
            DaEvents.Add(daEvent);
#endif
            foreach (var cloudEvent in _eventMapperService.GetCloudEventsFromDaEvent(daEvent))
            {
#if DEBUG
                CloudEvents.Add(cloudEvent);
#endif
                await _altinnEventService.PostEvent(cloudEvent);
            }

            //await _cursorService.UpdateCursor(new Cursor { Name = DaFeedCursorName, Value = daEvent.EventId });
        }
    }
}