using Microsoft.Extensions.DependencyInjection;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models.Da;
using oed_feedpoller.Models.Da.Dto;

namespace oed_feedpoller.Services.Hydrators;
public class HydratorFactory : IHydratorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public HydratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDaEventHydrator? GetHydrator(SchemaDefinition schemaDefinition)
    {
        switch (schemaDefinition.Id)
        {
            case Constants.SchemaDodsfallsak:
                return _serviceProvider.GetRequiredService<DodsbosakHydrator>();

            case Constants.SchemaFormuesfullmakt:
                return _serviceProvider.GetRequiredService<FormuesfullmaktHydrator>();
        }

        return null;
    }
}
