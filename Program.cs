using Microsoft.Extensions.Hosting;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Digdir.Oed.FeedPoller;
using Digdir.Oed.FeedPoller.Interfaces;
using Digdir.Oed.FeedPoller.Services;
using Digdir.Oed.FeedPoller.Settings;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config
            .AddEnvironmentVariables()
            .AddJsonFile("worker.json");

        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<FeedPoller>(false);
        }
    })
    // TODO Workaround for https://github.com/Azure/azure-functions-dotnet-worker/issues/1090
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
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder
            // Using preview package Microsoft.Azure.Functions.Worker.ApplicationInsights, see https://github.com/Azure/azure-functions-dotnet-worker/pull/944
            // Requires APPLICATIONINSIGHTS_CONNECTION_STRING being set. Note that host.json logging settings will have to be replicated to worker.json
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();

    }, options =>
    {
        //options.Serializer = new NewtonsoftJsonObjectSerializer();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<OedSettings>(context.Configuration.GetSection("OedSettings"));

        var mpSettings = context.Configuration.GetSection("MaskinportenSettings");
        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(Constants.DaHttpClient, mpSettings,
            clientDefinition =>
            {
                clientDefinition.ClientSettings.Scope = ScopesByPrefix("domstol", clientDefinition.ClientSettings.Scope);
                clientDefinition.ClientSettings.OverwriteAuthorizationHeader = false;
                clientDefinition.ClientSettings.Resource = Environment.GetEnvironmentVariable("MaskinportenSettings:DaResource");
            });

        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(Constants.EventsHttpClient, mpSettings,
            clientDefinition =>
            {
                clientDefinition.ClientSettings.Scope = ScopesByPrefix("altinn", clientDefinition.ClientSettings.Scope);
                clientDefinition.ClientSettings.Resource = Environment.GetEnvironmentVariable("MaskinportenSettings:OedEventsResource");
            });

        // Use if Redis not available locally
        //services.AddDistributedMemoryCache();
        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = context.Configuration.GetConnectionString("Redis");
        });
        
        services.AddSingleton<IDaEventFeedProxyService, DaEventFeedProxyService>();
    })
    .Build();

host.Run();

string ScopesByPrefix(string prefix, string scopes)
{
    var scopeList = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var filteredScopes = scopeList.Where(scope => scope.StartsWith($"{prefix}:")).ToList();

    return string.Join(' ', filteredScopes);
}

