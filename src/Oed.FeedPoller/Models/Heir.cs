using System.Text.Json.Serialization;

namespace Oed.FeedPoller.Models
{
    public class HeirRole
    {
        [JsonPropertyName("nin")]
        public string Nin { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
