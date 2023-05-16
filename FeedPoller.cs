using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Digdir.Oed.FeedPoller.Settings;

namespace Digdir.Oed.FeedPoller;

public class FeedPoller
{
    private readonly ILogger _logger;
    private readonly OedSettings _oedSettings;
    private readonly IHttpClientFactory _clientFactory;

    public FeedPoller(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory,
        IOptions<OedSettings> oedEventsSettings)
    {
        _logger = loggerFactory.CreateLogger<FeedPoller>();
        _oedSettings = oedEventsSettings.Value;
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

        if (Uri.IsWellFormedUriString(_oedSettings.OedEventsBaseUrl, UriKind.Absolute))
        {
            await PerformFeedPollAndUpdate();
        }
        else
        {
            _logger.LogError("Invalid configuration for OedEventsBaseUrl, should be an absolute url.");
        }

        _logger.LogInformation($"Next timer schedule at: {timerInfo.ScheduleStatus?.Next}");
    }

    private async Task PerformFeedPollAndUpdate()
    {
        HttpClient httpClient = _clientFactory.CreateClient(Constants.EventsHttpClient);
        string url = _oedSettings.OedEventsBaseUrl?.TrimEnd('/') + "/process";
        
        HttpResponseMessage result = await httpClient.PostAsync(url, null);
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to poll feed from {url}, status code: {result.StatusCode}");
        }
    }
}