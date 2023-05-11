using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Digdir.Oed.FeedPoller.Settings;

namespace Digdir.Oed.FeedPoller;

public class FeedPoller
{
    private readonly ILogger _logger;
    private readonly ApiSettings _apiSettings;
    private readonly IHttpClientFactory _clientFactory;

    public FeedPoller(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory,
        IOptions<ApiSettings> oedEventsSettings)
    {
        _logger = loggerFactory.CreateLogger<FeedPoller>();
        _apiSettings = oedEventsSettings.Value;
        _clientFactory = clientFactory;
    }

    [Function(nameof(FeedPoller))]
    public async Task RunAsync([TimerTrigger("*/5 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogDebug($"DA feed import executed at: {DateTime.Now}");

        if (timerInfo.IsPastDue)
        {
            _logger.LogWarning("DA feed import was not run on schedule");
        }

        if (!Helpers.ShouldRunUpdate())
        {
            _logger.LogDebug("Skipping update outside of busy hours");
            return;
        }

        await PerformFeedPollAndUpdate();

        _logger.LogInformation($"Next timer schedule at: {timerInfo.ScheduleStatus?.Next}");
    }

    private async Task PerformFeedPollAndUpdate()
    {
        HttpClient httpClient = _clientFactory.CreateClient(Constants.EventsHttpClient);
        HttpResponseMessage result = await httpClient.PostAsync(_apiSettings.ProcessDaEventFeedUrl, null);
    }
}