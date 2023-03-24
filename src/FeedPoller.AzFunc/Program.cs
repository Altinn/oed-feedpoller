using Microsoft.Extensions.Hosting;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Domstol.Hendelser.ApiClient;
using Oed.FeedPoller.Interfaces;
using Oed.FeedPoller.Models;
using Oed.FeedPoller.Services;
using Oed.FeedPoller.AzFunc.Proxy;
using Oed.FeedPoller.AzFunc;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config
            .AddEnvironmentVariables()
            .AddJsonFile("worker.json");

        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<FeedPollerFunction>(false);
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
        services.Configure<DaSettings>(context.Configuration.GetSection("DaSettings"));

        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(Constants.DaHttpClient, context.Configuration.GetSection("MaskinportenSettings"),
            clientDefinition =>
            {
                clientDefinition.ClientSettings.Scope = GetScopesByPrefix("domstol", clientDefinition.ClientSettings.Scope);
            });

        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(Constants.EventsHttpClient, context.Configuration.GetSection("MaskinportenSettings"),
            clientDefinition =>
            {
                clientDefinition.ClientSettings.Scope = GetScopesByPrefix("altinn", clientDefinition.ClientSettings.Scope);
            });

        // Use if Redis not available locally
        //services.AddDistributedMemoryCache();
        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = context.Configuration.GetConnectionString("Redis");
        });

        services.AddSingleton<IAltinnEventService, AltinnEventService>();
        services.AddSingleton<ICursorService, CursorService>();
        services.AddSingleton<IDaEventFeedService, DaEventFeedService>();
        services.AddSingleton<IDaEventFeedProxyService, DaEventFeedProxyService>();
        services.AddSingleton<DaEventFeedClient, DaEventFeedClient>();        
    })
    .Build();

host.Run();

string GetScopesByPrefix(string prefix, string scopes)
{
    var scopeList = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var filteredScopes = scopeList.Where(scope => scope.StartsWith($"{prefix}:")).ToList();

    return string.Join(' ', filteredScopes);
}

