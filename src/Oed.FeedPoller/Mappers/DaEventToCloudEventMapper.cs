using CloudNative.CloudEvents;
using Domstol.Hendelser.ApiClient;
using Oed.FeedPoller.Models;

namespace Oed.FeedPoller.Mappers;

public static class DaEventToCloudEventMapper
{
    private static readonly IReadOnlyDictionary<string, string> DaEventTypeMap =
        new Dictionary<string, string>()
        {
            { Constants.DaStatusAddedEventType, Constants.StatusAddedEventType },
            { Constants.DaHeirsAddedEventType, Constants.HeirsAddedEventType },
        };

    // Setup extension attributes
    private static readonly CloudEventAttribute ResourceAttrib = CloudEventAttribute.CreateExtension("resource", CloudEventAttributeType.String, ValidateUri);

    public static CloudEvent? MapToCloudEvent(this DaEvent daEvent, Sak sak)
    {
        if (daEvent.Type == Constants.DaHeirsAddedEventType)
        {
            List<HeirRole> roles = new();

            foreach (var part in sak.Parter)
            {
                var roleUrn = MapRole(part.Role);
                if (roleUrn is null)
                {
                    // TODO! This should be logged
                    continue;
                }

                if (part.Formuesfullmakt == PartFormuesfullmakt.ALTINN_DODSBO)
                {
                    roles.Add( new HeirRole
                    {
                        Nin = part.Nin,
                        Role = Constants.EstatePoaRole
                    });
                }

                // TODO! Finn ut om vi må forholde oss til `part.GodkjennerSkifteAttest`
                // Se https://altinn.slack.com/archives/C040J178EUC/p1680172452287319?thread_ts=1678195684.004959&cid=C040J178EUC

                if (sak.Skifteform == SakSkifteform.PRIVAT_SKIFTE && part.PaatarGjeldsansvar)
                {
                    roles.Add( new HeirRole
                    {
                        Nin = part.Nin,
                        Role = Constants.EstateProbateRole
                    });
                }
                
                roles.Add( new HeirRole
                {
                    Nin = part.Nin,
                    Role = roleUrn
                });
            }
            CloudEvent cloudEvent = new(CloudEventsSpecVersion.V1_0, new List<CloudEventAttribute> { ResourceAttrib })
            {
                Id = Guid.NewGuid().ToString(),
                Type = DaEventTypeMap.GetValueOrDefault(daEvent.Type),
                Source = daEvent.Data.@id,
                Subject = sak.Avdoede,
                DataContentType = "application/json",
                [ResourceAttrib] = Constants.ServiceResourceId,
                Data = roles,
            };
            return cloudEvent;
        }

        return null;
    }

    private static void ValidateUri(object value)
    {
        if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
        {
            throw new ArgumentNullException(nameof(value));
        }
        if (!Uri.TryCreate(stringValue, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException();
        }
    }

    private static readonly Dictionary<PartRole, string> PartRoleMapping = new()
    {
        { PartRole.PART_ANNEN_, "urn:digitaltdodsbo:arving:partAnnen"},
        { PartRole.GJENLEV_EKTEFELLE_PARTNER, "urn:digitaltdodsbo:arving:gjenlevEktefellePartner"},
        { PartRole.GJENLEV_PARTNER, "urn:digitaltdodsbo:arving:gjenlevPartner"},
        { PartRole.GJENLEV_SAMBOER, "urn:digitaltdodsbo:arving:gjenlevSamboer"},
        { PartRole.BARN, "urn:digitaltdodsbo:arving:barn"},
        { PartRole.BARNEBARN, "urn:digitaltdodsbo:arving:barnebarn"},
        { PartRole.SAERKULLSBARN, "urn:digitaltdodsbo:arving:saerkullsbarn"},
        { PartRole.SAERKULLSBARN_BARN, "urn:digitaltdodsbo:arving:saerkullsbarnBarn"},
        { PartRole.FAR, "urn:digitaltdodsbo:arving:far"},
        { PartRole.MOR, "urn:digitaltdodsbo:arving:mor"},
        { PartRole.SOESKEN, "urn:digitaltdodsbo:arving:soesken"},
        { PartRole.SOESKENS_BARN, "urn:digitaltdodsbo:arving:soeskensBarn"},
        { PartRole.SOESKENS_BARNEBARN, "urn:digitaltdodsbo:arving:soeskensBarnebarn"},
        { PartRole.HALV_SOESKEN, "urn:digitaltdodsbo:arving:halvSoesken"},
        { PartRole.HALV_SOESKENS_BARN, "urn:digitaltdodsbo:arving:halvSoeskensBarn"},
        { PartRole.FARFAR, "urn:digitaltdodsbo:arving:farfar"},
        { PartRole.FARMOR, "urn:digitaltdodsbo:arving:farmor"},
        { PartRole.MORFAR, "urn:digitaltdodsbo:arving:morfar"},
        { PartRole.MORMOR, "urn:digitaltdodsbo:arving:mormor"},
        { PartRole.ONKEL, "urn:digitaltdodsbo:arving:onkel"},
        { PartRole.TANTE, "urn:digitaltdodsbo:arving:tante"},
        { PartRole.FETTER, "urn:digitaltdodsbo:arving:fetter"},
        { PartRole.KUSINE, "urn:digitaltdodsbo:arving:kusine"},
        { PartRole.STATEN, "urn:digitaltdodsbo:arving:staten"},
        { PartRole.AVDOEDE, "urn:digitaltdodsbo:arving:avdoede"},
        { PartRole.FORDRINGSHAVER, "urn:digitaltdodsbo:arving:fordringshaver"},
        { PartRole.AVDOEDE_EKTEFELLE_PARTNER, "urn:digitaltdodsbo:arving:avdoedeEktefellePartner"}
    };
    
    private static string? MapRole(PartRole role)
    {
        PartRoleMapping.TryGetValue(role, out var urn);
        return urn;
    }
}
