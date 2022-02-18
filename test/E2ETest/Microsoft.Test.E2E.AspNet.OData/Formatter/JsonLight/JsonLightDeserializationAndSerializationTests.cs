//-----------------------------------------------------------------------------
// <copyright file="JsonLightDeserializationAndSerializationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight
{
    [EntitySet("UniverseEntity")]
    [Key("ID")]
    public class UniverseEntity
    {
        public UniverseEntity()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public string ID { get; set; }
        public int IntProperty { get; set; }
        public int? NullableIntProperty { get; set; }
        public bool BoolProperty { get; set; }
        public string StringProperty { get; set; }
        public ComplexType OptionalComplexProperty { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class UniverseEntityClient : UniverseEntity
    {
        public string string1 { get; set; }
        public string string2 { get; set; }
        public string string3 { get; set; }
        public string string4 { get; set; }
        public decimal number10 { get; set; }
        public decimal number10point5 { get; set; }
        public decimal number10e25 { get; set; }
        public bool boolean_true { get; set; }
        public bool boolean_false { get; set; }
    }

    public class ComplexType
    {
        public string Name { get; set; }
    }

    public class UniverseEntityController : InMemoryODataController<UniverseEntity, string>
    {
        public UniverseEntityController()
            : base("ID")
        {
        }
    }

    public class JsonLightDeserializationAndSerializationTests : ODataFormatterTestBase
    {
        public JsonLightDeserializationAndSerializationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public string AcceptHeader { get; set; }

        protected async Task PostAndGetShouldReturnSameEntity(UniverseEntity entity)
        {
            var uri = new Uri(this.BaseAddress);
            const string entitySetName = "UniverseEntity";
            await this.ClearRepositoryAsync(entitySetName);

            var ctx = WriterClient(uri, ODataProtocolVersion.V4);
            ctx.AddObject(entitySetName, entity);
            await ctx.SaveChangesAsync();

            // get collection of entities from repository
            ctx = ReaderClient(uri, ODataProtocolVersion.V4);
            DataServiceQuery<UniverseEntity> query = ctx.CreateQuery<UniverseEntity>(entitySetName);
            var entities = await Task.Factory.FromAsync(query.BeginExecute(null, null), (asyncResult) =>
            {
                return query.EndExecute(asyncResult);
            });

            var beforeUpdate = entities.ToList().First();
            AssertExtension.DeepEqual(entity, beforeUpdate);

            // clear repository
            await this.ClearRepositoryAsync(entitySetName);
        }

        public override DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.WriterClient(serviceRoot, protocolVersion);
            //new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            ctx.Format.UseJson(GetEdmModel(new ODataConventionModelBuilder()));

            return ctx;
        }

        public override DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = base.ReaderClient(serviceRoot, protocolVersion);
            //new JsonLightConfigurator(ctx, AcceptHeader).Configure();
            ctx.Format.UseJson(GetEdmModel(new ODataConventionModelBuilder()));

            return ctx;
        }

        public static TheoryDataSet<string> EntityData
        {
            get
            {
                var data = new TheoryDataSet<string>();
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
                    data.Add(header);
                }

                return data;
            }
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            var builder = configuration.CreateConventionModelBuilder();
            configuration.EnableODataSupport(GetEdmModel(builder));
        }

        protected static IEdmModel GetEdmModel(ODataConventionModelBuilder builder)
        {
            builder.EntitySet<UniverseEntity>("UniverseEntity")
                   .EntityType
                   .ComplexProperty(p => p.OptionalComplexProperty)
                   .IsOptional();

            return builder.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(EntityData))]
        public Task PutAndGetShouldReturnSameEntityJsonLight(string acceptHeader)
        {
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

            AcceptHeader = acceptHeader;
            return PostAndGetShouldReturnSameEntity(entity);
        }

        [Fact]
        public async Task SerializationTests()
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
                @"""number10point5@odata.type"":""#decimal""," +
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

            HttpResponseMessage response = await this.Client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(payload.ToLower().Replace("\"null\":null,",""), result.ToLower());
        }
    }
}
