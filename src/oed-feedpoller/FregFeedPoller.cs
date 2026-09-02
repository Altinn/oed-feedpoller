using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Digdir.Oed.FeedPoller.Settings;
using DigdirDigdir.Oed.FeedPoller.Constants;

namespace Digdir.Oed.FeedPoller;

public class FregFeedPoller
{
    private readonly ILogger _logger;
    private readonly OedSettings _oedSettings;
    private readonly IHttpClientFactory _clientFactory;

    public FregFeedPoller(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory,
        IOptions<OedSettings> oedEventsSettings)
    {
        _logger = loggerFactory.CreateLogger<FregFeedPoller>();
        _oedSettings = oedEventsSettings.Value;
        _clientFactory = clientFactory;
    }

    [Function(nameof(FregFeedPoller))]
    public async Task RunAsync([TimerTrigger("33 */3 * * * *")] TimerInfo timerInfo)
    {
        try
        {
            if (timerInfo.IsPastDue)
            {
                _logger.LogWarning("FREG feed import was not run on schedule");
            }

            if (!Helpers.ShouldRunUpdate())
            {
                _logger.LogDebug("Skipping update outside of busy hours");
                return;
            }

            if (Uri.IsWellFormedUriString(_oedSettings.OedEventsFregBaseUrl, UriKind.Absolute))
            {
                await PerformFeedPollAndUpdate();
            }
            else
            {
                _logger.LogError("Invalid configuration for OedEventsFregBaseUrl, should be an absolute url");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Next FREG timer schedule at: {Next}", timerInfo.ScheduleStatus?.Next);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to run FREG feedpoller.");
        }
    }

    private async Task PerformFeedPollAndUpdate()
    {
        HttpClient httpClient = _clientFactory.CreateClient(ClientConstants.EventsHttpClient);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string url = _oedSettings.OedEventsFregBaseUrl?.TrimEnd('/') + "/process";

        HttpResponseMessage result = await httpClient.PostAsync(url, null);
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to trigger processing of FREG event feed - POST {Url}, status code: {StatusCode}. Message: {Message}",
                url, result.StatusCode, await result.Content.ReadAsStringAsync());
        }
    }
}