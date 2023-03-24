using System.Dynamic;
using Oed.FeedPoller.Mappers;
using Oed.FeedPoller.Models;
using Domstol.Hendelser.ApiClient;

namespace FeedPoller.Tests
{
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
                        Role = PartRole.FAR
                    },
                    new Part
                    {
                        Nin = "24893249202",
                        Role = PartRole.MOR
                    }
                }
            };

            // Act
            var cloudEvent = DaEventToCloudEventMapper.MapToCloudEvent(daEvent, sak);

            // Assert
            Assert.NotNull(cloudEvent);
            Assert.NotNull(cloudEvent.Data);

            Assert.Equal("no.altinn.events.digitalt-dodsbo.heir-roles-updated", cloudEvent.Type);

            var resourceAttrib = cloudEvent.ExtensionAttributes.FirstOrDefault(attrib => attrib.Name == "resource");
            Assert.NotNull(resourceAttrib);
            Assert.Equal("urn:altinn:resource:dodsbo.domstoladmin.api", cloudEvent[resourceAttrib]);

            var resourceInstanceAttrib = cloudEvent.ExtensionAttributes.FirstOrDefault(attrib => attrib.Name == "resourceinstance");
            Assert.NotNull(resourceInstanceAttrib);
            Assert.Equal("fd635f35-6df5-4f39-8809-dbd797e40380", cloudEvent[resourceInstanceAttrib]);

            Assert.True(cloudEvent.Data is IEnumerable<HeirRole>);
            var heirList = new List<HeirRole>();
            heirList.AddRange((IEnumerable<HeirRole>)cloudEvent.Data);

            var far = heirList[0];
            Assert.NotNull(far);
            Assert.Equal("30923148511", far.Nin);
            Assert.Equal("FAR", far.Role);
            Assert.Equal("24916296424", far.RoleObjectNin);

            var mor = heirList[1];
            Assert.NotNull(mor);
            Assert.Equal("24893249202", mor.Nin);
            Assert.Equal("MOR", mor.Role);
            Assert.Equal("24916296424", mor.RoleObjectNin);            
        }        
    }
}