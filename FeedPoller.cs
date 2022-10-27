using System.Text.Json;
using Altinn.ApiClients.Maskinporten.Config;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Exceptions;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

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

    [Function(nameof(FeedPoller))]
    public async Task RunAsync([TimerTrigger("*/5 * * * *"
#if DEBUG
        , RunOnStartup = true
#endif
    )] TimerInfo timerInfo)
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

    private async Task PerformFeedPollAndUpdate()
    {
        var cursor = await _cursorService.GetCursor(DaFeedCursorName);
        await foreach (DaEvent daEvent in _daEventFeedService.GetEvents(cursor))
        {
            CloudEventRequestModel mappedEvent = _eventMapperService.GetCloudEventFromDaEvent(daEvent);
            try
            {
                await _altinnEventService.PostEvent(mappedEvent);
            }
            catch (InvalidAltinnEventException e)
            {
                // If the event service was able to authenticate/authorize the POST, but found its content invalid for some reason (aka 400 Bad Request), 
                // we continue to iterate and advance the cursor to avoid an invalid event to block the queue. We log the event as an error to be investigated.
                // All other errors (failed auth, timeouts etc) should not be caught and cause the process to fail and retry again on next timer iteration
                _logger.LogError("Event was rejected by Altinn Event, skipping. Exception message: {exceptionMessage} Serialized mapped event: {mappedEvent}", e.Message, JsonSerializer.Serialize(mappedEvent));
            }

            await _cursorService.UpdateCursor(new Cursor { Name = DaFeedCursorName, Value = daEvent.Id.ToString() });
        }
    }
}