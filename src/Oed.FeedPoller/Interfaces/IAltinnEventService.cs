using CloudNative.CloudEvents;

namespace Oed.FeedPoller.Interfaces;
public interface IAltinnEventService
{
    /// <summary>
    /// Post an event to the Altinn Event service to be distributed to its subscribers and made available on the event feed. Throws HttpRequestException in case of authentication/authorization, timeout or any other transient errors.
    /// </summary>
    /// <param name="cloudEvent">The cloud event to be sent</param>
    /// <exception cref="Exceptions.InvalidAltinnEventException">Thrown if the Altinn Event service rejected the event because of invalid format (ie. 400 Bad Request)</exception>
    /// <returns></returns>
    public Task PostEvent(CloudEvent cloudEvent);
}
