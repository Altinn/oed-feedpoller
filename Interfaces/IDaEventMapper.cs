using oed_feedpoller.Models;

namespace oed_feedpoller.Interfaces;
public interface IDaEventMapper
{
    public List<CloudEvent> GetMappedEvents(DaEvent daEvent);
}
