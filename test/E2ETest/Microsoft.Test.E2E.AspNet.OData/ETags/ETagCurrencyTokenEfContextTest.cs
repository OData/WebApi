// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class ETagCurrencyTokenEfContextTest : WebHostTestBase<ETagCurrencyTokenEfContextTest>
    {
        public ETagCurrencyTokenEfContextTest(WebHostTestFixture<ETagCurrencyTokenEfContextTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(DominiosController)};
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Dominio>("Dominios");
            builder.EntitySet<Server>("Servers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task NestedDollarSelectWorksOnCurrencyTokenProperty()
        {
            string expect = "{\r\n" +
"  \"@odata.context\":\"{XXXX}\",\"value\":[\r\n" +
"    {\r\n" +
"      \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"Descrizione\":\"Test1\",\"ServerAutenticazioneId\":\"1\",\"RECVER\":null,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"RECVER\":null\r\n" +
"      }\r\n" +
"    },{\r\n" +
"      \"@odata.etag\":\"W/\\\"MTA=\\\"\",\"Id\":\"2\",\"Descrizione\":\"Test2\",\"ServerAutenticazioneId\":\"2\",\"RECVER\":10,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"NQ==\\\"\",\"Id\":\"2\",\"RECVER\":5\r\n" +
"      }\r\n" +
"    }\r\n" +
"  ]\r\n" +
"}";
            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*([""{}\]])", "$1");

            expect = expect.Replace("{XXXX}", string.Format("{0}/odata/$metadata#Dominios(ServerAutenticazione(Id,RECVER))", BaseAddress.ToLowerInvariant()));

            var getUri = this.BaseAddress + "/odata/Dominios?$expand=ServerAutenticazione($select=Id,RECVER)";

            var response = await Client.GetAsync(getUri);

            response.EnsureSuccessStatusCode();

            Assert.NotNull(response.Content);

            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, payload);
        }
    }
}
