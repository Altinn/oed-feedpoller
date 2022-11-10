using oed_feedpoller.Models;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Interfaces;
public interface IDaEventHydrator
{
    public Task<DaEvent> GetHydratedEvent(string eventJsonFull, JsonPatchDocument jsonPatch);
}
