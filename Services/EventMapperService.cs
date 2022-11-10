using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;
public class EventMapperService : IEventMapperService
{
    private readonly IMapperFactory _mapperFactory;
    private const string CloudEventSource = "urn:altinn:events:digitalt-dodsbo:domstoladmin";

    public EventMapperService(IMapperFactory mapperFactory)
    {
        _mapperFactory = mapperFactory;
    }

    /// <inheritdoc/>
    public List<CloudEvent> GetCloudEventsFromDaEvent(DaEvent daEvent)
    {
        var mapper = _mapperFactory.GetMapper(daEvent.Type);
        if (mapper == null)
        {
            throw new Exception($"Missing mapper for type {daEvent.Type}");
        }

        var mappedEvents = mapper.GetMappedEvents(daEvent);
        var idx = 0;
        foreach (var mappedEvent in mappedEvents)
        {
            mappedEvent.Id = daEvent.EventId + "_" + idx;
            mappedEvent.Source = new Uri(CloudEventSource);
            mappedEvent.Subject = "/person/" + daEvent.Estate;
            idx++;
        }

        return mappedEvents;
    }
}
