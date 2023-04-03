namespace Oed.FeedPoller.Models;
public static class Constants
{
    public const string DaHttpClient = "DaHttpClient";
    public const string EventsHttpClient = "EventsHttpClient";

    public const string DaStatusAddedEventType = "DODSFALLSAK-STATUS_LAGT_TIL";
    public const string DaHeirsAddedEventType = "PARTER_LAGT_TIL";
    
    public const string StatusAddedEventType = "no.altinn.events.digitalt-dodsbo.v1.case-status-updated";
    public const string HeirsAddedEventType = "no.altinn.events.digitalt-dodsbo.v1.heir-roles-updated";

    public const string EstatePoaRole = "urn:digitaltdodsbo:formuesfullmakt";
    
    public const string ServiceResourceId = "urn:altinn:resource:dodsbo.domstoladmin.api";
}
