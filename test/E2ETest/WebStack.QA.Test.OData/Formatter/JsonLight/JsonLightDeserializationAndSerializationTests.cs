using System;
using System.Web.Http;
using Microsoft.OData.Client;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightDeserializationAndSerializationTests : DeserializationAndSerializationTests
    {
        public string AcceptHeader { get; set; }

        public override DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            //new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            ctx.Format.UseJson(GetEdmModel());

            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.ReaderClient(serviceRoot, protocolVersion);
            //new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            ctx.Format.UseJson(GetEdmModel());

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
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=full",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false",
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

            configuration.EnableODataSupport(GetEdmModel());
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
