using Microsoft.Azure.Functions.Worker.Http;
using oed_feedpoller.Models;
using oed_feedpoller.Models.Da;

namespace oed_feedpoller.Interfaces;
public interface IDaEventFeedService
{
    /// <summary>
    /// Returns an iterator for a list of events currently available at DA from the specified cursor. 
    /// </summary>
    /// <param name="cursor">The cursor from which we need to fetch events</param>
    /// <returns>An iterator for the events that should be processed</returns>
    public IAsyncEnumerable<DaEvent> GetEvents(Cursor cursor);

    /// <summary>
    /// Proxy function that we use in non-staging environments to access the DA feed which is firewalled
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<HttpResponseData> ProxyRequest(HttpRequestData request);
}
