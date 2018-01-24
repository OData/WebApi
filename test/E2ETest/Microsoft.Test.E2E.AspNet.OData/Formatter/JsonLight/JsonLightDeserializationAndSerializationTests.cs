// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.OData.Client;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight
{
    public class JsonLightDeserializationAndSerializationTests : DeserializationAndSerializationTests
    {
        public JsonLightDeserializationAndSerializationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

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

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
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
