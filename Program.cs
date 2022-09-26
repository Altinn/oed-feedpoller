using Microsoft.Extensions.Hosting;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.ApiClients.Maskinporten.Extensions;
using Microsoft.Extensions.DependencyInjection;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition>(
            context.Configuration.GetSection("EventsMaskinportenSettings"),
            "EventsHttpClient");

        services.AddSingleton<IAltinnEventService, AltinnEventService>();
        services.AddSingleton<ICursorService, CursorService>();
        services.AddSingleton<IDaEventFeedService, DaEventFeedService>();
        services.AddSingleton<IEventMapperService, EventMapperService>();

    })
    .Build();

host.Run();
