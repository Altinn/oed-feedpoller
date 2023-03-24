using CloudNative.CloudEvents;
using Domstol.Hendelser.ApiClient;
using Oed.FeedPoller.Models;

namespace Oed.FeedPoller.Interfaces;
public interface IDaEventFeedService
{
    /// <summary>
    /// Returns an iterator for a list of events currently available at DA from the specified cursor. 
    /// </summary>
    /// <param name="cursor">The cursor from which we need to fetch events</param>
    /// <returns>An iterator for the events that should be processed</returns>
    public Task<IEnumerable<CloudEvent>> GetEvents(Cursor cursor);

    public IEnumerable<CloudEvent> ConvertEventFeed(ICollection<DaEvent> daEventFeed, ICollection<Sak> daCases);
}
