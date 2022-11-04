using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;
public class EventMapperService : IEventMapperService
{
    private const string CloudEventSource = "urn:digitaltdodsbo";

    /// <inheritdoc/>
    public CloudEventRequestModel GetCloudEventFromDaEvent(DaEvent daEvent)
    {
        return new CloudEventRequestModel
        {
            Source = new Uri(CloudEventSource),
            AlternativeSubject = "person/" + daEvent.Estate,
            Type = daEvent.Type,
            Data = daEvent.EventData
        };
    }
}
