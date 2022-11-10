using Microsoft.Extensions.DependencyInjection;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller.Services.Mappers;
public class MapperFactory : IMapperFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MapperFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public IDaEventMapper? GetMapper(string eventType)
    {
        switch (eventType)
        {
            case Constants.EventTypeDodsfallsak:
                return _serviceProvider.GetRequiredService<DodsbosakMapper>();

            case Constants.EventTypeFormuesfullmakt:
                return _serviceProvider.GetRequiredService<FormuesfullmaktMapper>();
        }

        return null;
    }
}
