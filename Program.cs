using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Services;
using Digdir.Oed.FeedPoller;
using Digdir.Oed.FeedPoller.Interfaces;
using Digdir.Oed.FeedPoller.Services;
using Digdir.Oed.FeedPoller.Settings;
using Digdir.Oed.FeedPoller.Constants;
using OpenTelemetry;
using OpenTelemetry.Logs;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config
            .AddEnvironmentVariables()
            .AddJsonFile("worker.json", optional: true)
            .AddJsonFile("host.json", optional: true);

        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<FeedPoller>(optional: true);
        }
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(loggingBuilder =>
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.ColorBehavior = LoggerColorBehavior.Enabled;
                options.SingleLine = true;
            });
        }
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // OpenTelemetry med Application Insights-export
        services.AddOpenTelemetry()
            .UseFunctionsWorkerDefaults()
            .ConfigureResource(r => r.AddService("oed-feedpoller"))
            .WithTracing(tb => tb
                .AddHttpClientInstrumentation()
                .AddSource("Microsoft.Azure.Functions.Worker")
                .AddAzureMonitorTraceExporter(o =>
                    o.ConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            .WithMetrics(mb => mb
                .AddHttpClientInstrumentation()
                .AddAzureMonitorMetricExporter(o =>
                    o.ConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"]));
        services.Configure<OpenTelemetryLoggerOptions>(logging =>
        {            
            logging.IncludeFormattedMessage = true;
            logging.ParseStateValues = true;
        });

        // Last inn OED-innstillinger
        services.Configure<OedSettings>(config.GetSection("OedSettings"));

        // Maskinporten-klienter
        var mpSettings = config.GetSection("MaskinportenSettings");

        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(
            ClientConstants.DaHttpClient, mpSettings,
            client =>
            {
                client.ClientSettings.Scope = ScopesByPrefix("domstol", client.ClientSettings.Scope);
                client.ClientSettings.OverwriteAuthorizationHeader = false;
                client.ClientSettings.Resource = config["MaskinportenSettings:DaResource"];
            });

        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(
            ClientConstants.EventsHttpClient, mpSettings,
            client =>
            {
                client.ClientSettings.Scope = "altinn:dd:internalevents";
                client.ClientSettings.Resource = config["MaskinportenSettings:OedEventsResource"];
            });

        // Redis eller fallback
        var redisConn = config.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = redisConn;
            });
        }

        // Tjenester
        services.AddSingleton<IDaEventFeedProxyService, DaEventFeedProxyService>();
    })
    .Build();

Console.WriteLine("Starting Digdir.Oed.FeedPoller host with OpenTelemetry and .NET 8...");

host.Run();

static string ScopesByPrefix(string prefix, string scopes)
{
    var scopeList = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var filteredScopes = scopeList.Where(scope => scope.StartsWith($"{prefix}:")).ToList();
    return string.Join(' ', filteredScopes);
}
