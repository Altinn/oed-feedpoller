using System.Net.Http.Headers;
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
        _logger.LogDebug("DA feed import executed at: {Now}", DateTime.Now);

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
            _logger.LogError("Invalid configuration for OedEventsBaseUrl, should be an absolute url");
        }

        _logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus?.Next);
    }

    private async Task PerformFeedPollAndUpdate()
    {
        HttpClient httpClient = _clientFactory.CreateClient(Constants.EventsHttpClient);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string url = _oedSettings.OedEventsBaseUrl?.TrimEnd('/') + "/process";
        
        HttpResponseMessage result = await httpClient.PostAsync(url, null);
        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError("Bearer token: {AuthToken}", result.RequestMessage!.Headers.Authorization);
            _logger.LogError("Failed to trigger processing of DA event feed - POST {Url}, status code: {StatusCode}. Message: {Message}", 
                url, result.StatusCode, await result.Content.ReadAsStringAsync());
        }
    }
}