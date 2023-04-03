using System.Dynamic;
using Domstol.Hendelser.ApiClient;
using Oed.FeedPoller.Mappers;
using Oed.FeedPoller.Models;

namespace Oed.FeedPoller.Tests;

public class DaEventToCloudEventMapperTests
{
    [Fact]
    public void Map_DaParterLagtTil_ReturnsValidHeirsAddedEvent()
    {
        dynamic eventData = new ExpandoObject();
        eventData.@id = new Uri("https://hendelsesliste.test.domstol.no/api/objects/fd635f35-6df5-4f39-8809-dbd797e40380");
            
        // Arrange
        DaEvent daEvent = new()
        {
            Specversion = "1.0",
            Id = "fd635f35-6df5-4f39-8809-dbd797e40380",
            Source = "https://domstol.no",
            Type = "PARTER_LAGT_TIL",
            Datacontenttype = "application/json",
            Time = DateTimeOffset.Parse("2023-03-15T10:57:53.787325282+01:00"),
            Data = eventData,
        };

        Sak sak = new()
        {
            SakId = new Guid("fd635f35-6df5-4f39-8809-dbd797e40380"),
            Avdoede = "24916296424",
            Parter = new[]
            {
                new Part
                {
                    Nin = "30923148511",
                    Role = PartRole.GJENLEV_EKTEFELLE_PARTNER,
                    Formuesfullmakt = PartFormuesfullmakt.ALTINN_DODSBO
                },
                new Part
                {
                    Nin = "24893249202",
                    Role = PartRole.BARN,
                    Formuesfullmakt = PartFormuesfullmakt.INGEN
                }
            }
        };

        // Act
        var cloudEvent = daEvent.MapToCloudEvent(sak);

        // Assert
        Assert.NotNull(cloudEvent);
        Assert.NotNull(cloudEvent.Data);

        Assert.Equal("no.altinn.events.digitalt-dodsbo.v1.heir-roles-updated", cloudEvent.Type);

        var resourceAttrib = cloudEvent.ExtensionAttributes.FirstOrDefault(attrib => attrib.Name == "resource");
        Assert.NotNull(resourceAttrib);
        Assert.Equal("urn:altinn:resource:dodsbo.domstoladmin.api", cloudEvent[resourceAttrib]);
        Assert.Equal("24916296424", cloudEvent.Subject);

        Assert.True(cloudEvent.Data is IEnumerable<HeirRole>);
        var heirList = new List<HeirRole>();
        heirList.AddRange((IEnumerable<HeirRole>)cloudEvent.Data);

        var ektefelleFormuesfullmakt = heirList[0];
        Assert.NotNull(ektefelleFormuesfullmakt);
        Assert.Equal("30923148511", ektefelleFormuesfullmakt.Nin);
        Assert.Equal("urn:digitaltdodsbo:formuesfullmakt", ektefelleFormuesfullmakt.Role);
            
        var ektefelleArving = heirList[1];
        Assert.NotNull(ektefelleArving);
        Assert.Equal("30923148511", ektefelleArving.Nin);
        Assert.Equal("urn:digitaltdodsbo:arving:gjenlevEktefellePartner", ektefelleArving.Role);

        var barnArving = heirList[2];
        Assert.NotNull(barnArving);
        Assert.Equal("24893249202", barnArving.Nin);
        Assert.Equal("urn:digitaltdodsbo:arving:barn", barnArving.Role);
        Assert.Equal("24916296424", cloudEvent.Subject);            
    }        
}