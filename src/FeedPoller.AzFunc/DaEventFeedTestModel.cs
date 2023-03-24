using Domstol.Hendelser.ApiClient;

namespace Oed.FeedPoller.AzFunc
{
    internal class DaEventFeedTestModel
    {
        public ICollection<ICollection<DaEvent>> DaEventList { get; set; }

        public ICollection<Sak> DaCaseList { get; set; }

    }
}
