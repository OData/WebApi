using System;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Net.Http.Headers;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightDeserializationAndSerializationTests : DeserializationAndSerializationTests
    {
        public string AcceptHeader { get; set; }

        public override DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            var ctx = base.ReaderClient(serviceRoot, protocolVersion);
            new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            return ctx;
        }

        public static TheoryDataSet<UniverseEntity, string> EntityData
        {
            get
            {
                var data = new TheoryDataSet<UniverseEntity, string>();
                var entity = new UniverseEntity()
                {
                    ID = "1",
                    OptionalComplexProperty = null
                };
                var acceptHeaders = new string[] 
                {
                    "application/json;odata=minimalmetadata;streaming=true",
                    "application/json;odata=minimalmetadata;streaming=false",
                    "application/json;odata=minimalmetadata",
                    "application/json;odata=fullmetadata;streaming=true",
                    "application/json;odata=fullmetadata;streaming=false",
                    "application/json;odata=fullmetadata",
                    "application/json;streaming=true",
                    "application/json;streaming=false",
                    "application/json",
                };
                foreach (var header in acceptHeaders)
                {
                    data.Add(entity, header);
                }

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
        public void PutAndGetShouldReturnSameEntityJsonLight(UniverseEntity entity, string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            PostAndGetShouldReturnSameEntity(entity);
        }
    }
}
