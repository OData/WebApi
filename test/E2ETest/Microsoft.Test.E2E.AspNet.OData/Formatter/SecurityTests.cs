// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    [EntitySet("Security_NestedModel")]
    [Key("ID")]
    public class Security_NestedModel
    {
        public int ID { get; set; }
        public Security_NestedModel Nest { get; set; }
    }

    [EntitySet("Security_ArrayModel")]
    [Key("ID")]
    public class Security_ArrayModel
    {
        public int ID { get; set; }
        public List<string> StringArray { get; set; }
        public List<Security_ComplexModel> ComplexArray { get; set; }
        public List<Security_NestedModel> NavigationCollection { get; set; }
    }

    public class Security_ComplexModel
    {
        public string Name { get; set; }
    }

    public class Security_NestedModelController : InMemoryODataController<Security_NestedModel, int>
    {
        public Security_NestedModelController()
            : base("ID")
        {
        }
    }

    public class Security_ArrayModelController : InMemoryODataController<Security_ArrayModel, int>
    {
        public Security_ArrayModelController()
            : base("ID")
        {
        }
    }

    public class DosSecurityTests : WebHostTestBase
    {
        public DosSecurityTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var selfConfig = configuration as HttpSelfHostConfiguration;
            if (selfConfig != null)
            {
                selfConfig.MaxReceivedMessageSize = selfConfig.MaxBufferSize = int.MaxValue;
            }

            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel());
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Security_NestedModel>("Security_NestedModel");
            builder.EntitySet<Security_ArrayModel>("Security_ArrayModel");

            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task DeepNestedObjectShouldBeBlockedByRecursionCheck(string mediaType)
        {
            var root = new Security_NestedModel();
            var nest = root;
            for (int i = 0; i < 1000; i++)
            {
                nest.Nest = new Security_NestedModel();
                nest = nest.Nest;
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_NestedModel");
            request.Content = new ObjectContent<Security_NestedModel>(root, new JsonMediaTypeFormatter(), MediaTypeHeaderValue.Parse(mediaType));
            var response = await this.Client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("The depth limit for entries in nested expanded navigation links was reached", content);
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task BigArrayObjectWithJsonShoulNotdMakeServerHang(string mediaType)
        {
            var model = new Security_ArrayModel();
            List<string> stringList = new List<string>();
            List<Security_ComplexModel> complexList = new List<Security_ComplexModel>();
            List<Security_NestedModel> navigationList = new List<Security_NestedModel>();
            for (int i = 0; i < 10000; i++)
            {
                stringList.Add(string.Empty);
                complexList.Add(new Security_ComplexModel());
                navigationList.Add(new Security_NestedModel());
            }
            model.StringArray = stringList;
            model.ComplexArray = complexList;
            model.NavigationCollection = navigationList;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");
            request.Content = new ObjectContent<Security_ArrayModel>(model, new JsonMediaTypeFormatter(), MediaTypeHeaderValue.Parse(mediaType));
            var response = await this.Client.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task BigDataServiceVersionHeaderShouldBeRejected()
        {
            var model = new Security_ArrayModel();

            AttackStringBuilder asb = new AttackStringBuilder();
            asb.Append("3.0").Repeat("0", 100000);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.BaseAddress + "/Security_ArrayModel");

            request.Content = new ObjectContent<Security_ArrayModel>(model, new JsonMediaTypeFormatter(), MediaTypeHeaderValue.Parse("application/json"));
            request.Headers.Add("DataServiceVersion", asb.ToString());
            var response = await this.Client.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode);
        }
    }
}
