using CloudNative.CloudEvents;
using Domstol.Hendelser.ApiClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oed.FeedPoller.Extensions;
using Oed.FeedPoller.Interfaces;
using Oed.FeedPoller.Mappers;
using Oed.FeedPoller.Models;

namespace Oed.FeedPoller.Services;
public class DaEventFeedService : IDaEventFeedService
{
    private readonly DaEventFeedClient _daEventFeedClient;
    private readonly ILogger<DaEventFeedService> _logger;
    private readonly DaSettings _settings;

    public DaEventFeedService(
        DaEventFeedClient daEventFeedApiClient,
        IOptions<DaSettings> settings,
        ILoggerFactory loggerFactory)
    {
        _daEventFeedClient = daEventFeedApiClient;
        _logger = loggerFactory.CreateLogger<DaEventFeedService>();
        _settings = settings.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CloudEvent>> GetEvents(Cursor cursor)
    {
        DateTime after = DateTime.Parse(cursor.Value ?? string.Empty);

        _logger.LogInformation($"Getting events after: ${after}");
        var daEventFeed = await _daEventFeedClient.HendelseslisteAsync(after, _settings.MaxItemsPerPoll);

        // implement two-pass approach to retrieve each /object/ only once (within this scope)
        Dictionary<Guid, Sak> daCases = new Dictionary<Guid, Sak>();
        List<Guid> allCaseIds = new List<Guid>();

        // flatten event feed structure
        var daEvents = daEventFeed.SelectMany(list => list).Select(list2 => list2).ToList();

        // convert event Ids into valid case Guids, ignoring non-guids
        var caseIds = daEvents.Select(daCase => daCase.Id.StrToGuid()).SkipNulls().ToList();

        foreach (var caseId in allCaseIds)
        {
            daCases[caseId] = await _daEventFeedClient.ObjectsAsync(caseId);
        }

        var outgoingEvents = ConvertEventFeed(daEvents, daCases.Values.SkipNulls().ToList());

        return outgoingEvents;
    }



    public IEnumerable<CloudEvent> ConvertEventFeed(ICollection<DaEvent> daEventFeed, ICollection<Sak> daCases)
    {
        List<CloudEvent> outgoingEvents = new List<CloudEvent>();

        foreach (DaEvent daEvent in daEventFeed)
        {
            var isGuid = Guid.TryParse(daEvent.Id, out Guid caseId);
            Sak? daSak = isGuid ? daCases.FirstOrDefault(daCase => daCase.SakId == caseId) : null;
            if (daSak != null)
            {
                var cloudEvent = daEvent.MapToCloudEvent(daSak);
                if (cloudEvent != null)
                {
                    outgoingEvents.Add(cloudEvent);
                }
            }
        }

        return outgoingEvents;
    }

    
}
