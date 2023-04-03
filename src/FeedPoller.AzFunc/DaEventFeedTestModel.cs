using Domstol.Hendelser.ApiClient;

namespace Oed.FeedPoller.AzFunc
{
    internal class DaEventFeedTestModel
    {
        public ICollection<ICollection<DaEvent>> DaEventList { get; set; } = null!;

        public ICollection<Sak> DaCaseList { get; set; } = null!;

    }
}
