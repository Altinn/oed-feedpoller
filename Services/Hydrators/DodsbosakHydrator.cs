using System.Text.Json;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Models.BusinessObjects;
using oed_feedpoller.Models.Da;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Services.Hydrators;
public class DodsbosakHydrator : IDaEventHydrator
{
    private readonly IDaApiClient _daApiClient;
    private readonly ILogger<DodsbosakHydrator> _logger;

    public DodsbosakHydrator(ILoggerFactory loggerFactory, IDaApiClient daApiClient)
    {
        _daApiClient = daApiClient;
        _logger = loggerFactory.CreateLogger<DodsbosakHydrator>();
    }

    public async Task<DaEvent> GetHydratedEvent(string eventJson, JsonPatchDocument jsonPatchDocument)
    {
        var dodsfallsak = JsonSerializer.Deserialize<Dodsfallsak>(eventJson)!;
        var candidateList = new CandidateList();

        foreach (var patch in jsonPatchDocument.Patch)
        {
            if (patch.Op == JsonPatchOp.Remove)
            {
                _logger.LogWarning("remove");
            }
        }

        foreach (var part in dodsfallsak.Parties)
        {
            var person = await _daApiClient.GetCachedAsync<Person>(part.PartyCid.Uri);
            candidateList.Candidates.Add(new Candidate
            {
                Role = part.Role,
                Ssn = person.Ssn
            });
        }

        var deceased = candidateList.Candidates.First(x => x.Role == Constants.RoleDeceased);
        candidateList.Candidates.Remove(deceased);

        var daEvent = new DaEvent
        {
            Type = Constants.EventTypeDodsfallsak,
            Estate = deceased.Ssn,
            EventData = candidateList
        };

        return daEvent;

    }
}
