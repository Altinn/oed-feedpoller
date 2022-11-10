using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Models.BusinessObjects;

namespace oed_feedpoller.Services.Mappers;
public class DodsbosakMapper : IDaEventMapper
{
    public List<CloudEvent> GetMappedEvents(DaEvent daEvent)
    {
        var candidateList = (CandidateList)daEvent.EventData!;

        /*
        var candidateList = (CandidateList)daEvent.EventData!;
        var result = new List<CloudEvent>
        {
            new()
            {
                Type = "estateInstanceCreatedOrUpdated"
            }
        };

        foreach (var candidate in candidateList.Candidates)
        {
            result.Add(new()
            {
                Type = "roleAssignment",
                Data = new
                {
                    recipient = candidate.Ssn,
                    roleCode = candidate.Role
                }
            });
        }

        return result;
        */
        var cloudEventRoleRecipients = new List<object>();
        foreach (var candidate in candidateList.Candidates)
        {
            cloudEventRoleRecipients.Add(new
            {
                recipient = candidate.Ssn,
                roleCode = candidate.Role
            });
        }

        return new List<CloudEvent>
        {
            new()
            {
                Type = "estateInstanceCreatedOrUpdated",
                Data = cloudEventRoleRecipients
            }
        };

    }
}
