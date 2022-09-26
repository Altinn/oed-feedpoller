using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;
public class EventMapperService : IEventMapperService
{
    /// <inheritdoc/>
    public CloudEventRequestModel GetCloudEventFromDaEvent(DaEvent daEvent)
    {
        throw new NotImplementedException();
    }
}
