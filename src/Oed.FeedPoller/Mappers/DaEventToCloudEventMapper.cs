using CloudNative.CloudEvents;
using Domstol.Hendelser.ApiClient;
using Oed.FeedPoller.Models;
using System.Dynamic;

namespace Oed.FeedPoller.Mappers
{
    public static class DaEventToCloudEventMapper
    {

        public static readonly string DaStatusAddedEventType = "DODSFALLSAK-STATUS_LAGT_TIL";
        public static readonly string DaHeirsAddedEventType = "PARTER_LAGT_TIL";

        public static readonly string StatusAddedEventType = "no.altinn.events.digitalt-dodsbo.case-status-updated";
        public static readonly string HeirsAddedEventType = "no.altinn.events.digitalt-dodsbo.heir-roles-updated";


        internal static readonly IReadOnlyDictionary<string, string> daEventTypeMap =
            new Dictionary<string, string>()
            {
                { DaStatusAddedEventType, StatusAddedEventType },
                { DaHeirsAddedEventType, HeirsAddedEventType },
            };

        // Setup extension attributes
        internal static CloudEventAttribute resourceAttrib = CloudEventAttribute.CreateExtension("resource", CloudEventAttributeType.String, ValidateUri);
        internal static CloudEventAttribute resourceInstanceAttrib = CloudEventAttribute.CreateExtension("resourceinstance", CloudEventAttributeType.String, ValidateResourceInstance);


        public static CloudEvent? MapToCloudEvent(this DaEvent daEvent, Sak sak)
        {
            if (daEvent.Type == DaHeirsAddedEventType)
            {
                List<HeirRole> roles = new();

                foreach (var part in sak.Parter)
                {
                    HeirRole heirData = new()
                    {
                        Nin = part.Nin,
                        Role = part.Role.ToString(),
                        RoleObjectNin = sak.Avdoede,
                    };
                    roles.Add(heirData);
                }
                CloudEvent cloudEvent = new(CloudEventsSpecVersion.V1_0, new List<CloudEventAttribute> { resourceAttrib, resourceInstanceAttrib })
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = daEventTypeMap.GetValueOrDefault(daEvent.Type),
                    Source = daEvent.Data.@id,
                    DataContentType = "application/json",
                    [resourceAttrib] = "urn:altinn:resource:dodsbo.domstoladmin.api",
                    [resourceInstanceAttrib] = daEvent.Id,
                    Data = roles,
                };
                return cloudEvent;
            }

            return null;
        }

        private static void ValidateUri(object value)
        {
            if (value == null || !(value is string) || string.IsNullOrEmpty((string)value))
            {
                throw new ArgumentNullException("value");
            }
            if (!Uri.TryCreate((string)value, UriKind.Absolute, out Uri? result))
            {
                throw new InvalidOperationException();
            }
        }

        private static void ValidateResourceInstance(object value)
        {
            if (value == null || !(value is string) || string.IsNullOrEmpty((string)value))
            {
                throw new ArgumentNullException("value");
            }
        }
    }
}
