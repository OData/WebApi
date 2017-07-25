using System;
using System.Web.Http;
using Microsoft.OData.Client;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit.Extensions;
using Xunit;
using System.Net.Http;
using System.Net.Http.Headers;

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
                //var entity = new UniverseEntityClient()
                var entity = new UniverseEntity()
                {
                    ID = "1",
                    OptionalComplexProperty = null,
                    //string1 = @"""[t@xt""",
                    //string2 = @"""'[t@xt""",
                    //string3 =  @"""}t@xt{""",
                    //string4 = @"""[t@xt'',,][']other::]mo}{retext""",
                    //number10 = 10,
                    //number10point5 = 10.5M,
                    //number10e25 = 10e25M,
                    //boolean_true = true,
                    //boolean_false = false
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
//        public void PutAndGetShouldReturnSameEntityJsonLight(UniverseEntityClient entity, string acceptHeader)
        public void PutAndGetShouldReturnSameEntityJsonLight(UniverseEntity entity, string acceptHeader)
        {
            AcceptHeader = acceptHeader;
            PostAndGetShouldReturnSameEntity(entity);
        }

        [Fact(Skip = "VSTS AX: Null elimination")]
        public void SerializationTests()
        {
            string requestUri = this.BaseAddress + "/UniverseEntity";
            string payload = @"{""@odata.context"":""" + this.BaseAddress + @"/$metadata#UniverseEntity/$entity""," +
                @"""ID"":""1""," +
                @"""IntProperty"":0,""NullableIntProperty"":null,""BoolProperty"":false,""StringProperty"":null," +
                @"""string1"":""[t@xt""," +
                @"""string2"":""'[t@xt""," +
                @"""string3"":""}t@xt{""," +
                @"""string4"":""[t@xt'',,][']other::]mo}{retext""," +
                @"""number10"":10," +
                @"""number10point5"":10.5," +
                @"""number10e25"":1E+26," +
                @"""boolean_true"":true," +
                @"""boolean_false"":false," +
                @"""null"":null," +
                @"""empty"":""""," +
                @"""OptionalComplexProperty"":null}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            HttpResponseMessage response = this.Client.SendAsync(request).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            Assert.Equal(payload.ToLower().Replace("\"null\":null,",""), result.ToLower());
        }
    }
}
