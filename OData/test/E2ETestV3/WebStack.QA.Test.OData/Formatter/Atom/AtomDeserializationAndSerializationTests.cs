using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.Atom
{
    public class AtomDeserializationAndSerializationTests : DeserializationAndSerializationTests
    {
        public static TheoryDataSet<UniverseEntity> EntityData
        {
            get
            {
                var data = new TheoryDataSet<UniverseEntity>();
                data.Add(new UniverseEntity()
                {
                    ID = "1",
                    OptionalComplexProperty = null
                });

                return data;
            }
        }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Theory]
        [PropertyData("EntityData")]
        public void PutAndGetShouldReturnSameEntityAtom(UniverseEntity entity)
        {
            PostAndGetShouldReturnSameEntity(entity);
        }
    }
}
