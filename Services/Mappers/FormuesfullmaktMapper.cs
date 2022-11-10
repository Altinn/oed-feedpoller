using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Models.BusinessObjects;

namespace oed_feedpoller.Services.Mappers;
public class FormuesfullmaktMapper : IDaEventMapper
{
    public List<CloudEvent> GetMappedEvents(DaEvent daEvent)
    {
        var eventData = (FormuesFullmaktRecipient)daEvent.EventData!;

        return new List<CloudEvent>
        {
            new()
            {
                Type = "roleAssignment",
                Data = new
                {
                    recipient = eventData.Ssn,
                    roleCode = "formuesfullmakt"
                }
            }
        };
    }
}
