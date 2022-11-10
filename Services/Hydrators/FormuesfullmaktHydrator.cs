using System.Text.Json;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;
using oed_feedpoller.Models.BusinessObjects;
using oed_feedpoller.Models.Da;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Services.Hydrators;

public class FormuesfullmaktHydrator : IDaEventHydrator
{
    private readonly IDaApiClient _daApiClient;
    private readonly ILogger<FormuesfullmaktHydrator> _logger;

    public FormuesfullmaktHydrator(ILoggerFactory loggerFactory, IDaApiClient daApiClient)
    {
        _daApiClient = daApiClient;
        _logger = loggerFactory.CreateLogger<FormuesfullmaktHydrator>();
    }

    public async Task<DaEvent> GetHydratedEvent(string eventJson, JsonPatchDocument jsonPatchDocument)
    {
        var formuesFullmakt = JsonSerializer.Deserialize<Formuesfullmakt>(eventJson)!;

        var deceased = _daApiClient.GetCachedAsync<Person>(formuesFullmakt.Deceased.Uri);
        var recepient = _daApiClient.GetCachedAsync<Person>(formuesFullmakt.Recipient.Uri);

        await Task.WhenAll(deceased, recepient);

        var daEvent = new DaEvent
        {
            Type = Constants.EventTypeFormuesfullmakt,
            Estate = deceased.Result!.Ssn,
            EventData = new FormuesFullmaktRecipient
            {
                Ssn = recepient.Result!.Ssn
            }
        };

        return daEvent;
    }
}