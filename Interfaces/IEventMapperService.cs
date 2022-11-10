using oed_feedpoller.Models;

namespace oed_feedpoller.Interfaces;
public interface IEventMapperService
{
    /// <summary>
    /// Handles mapping of the DA event to a one or more CloudEvents that can be posted to the Altinn Event Service
    /// </summary>
    /// <param name="daEvent">The DA event to be processed</param>
    /// <returns>The cloud event</returns>
    public List<CloudEvent> GetCloudEventsFromDaEvent(DaEvent daEvent);
}
