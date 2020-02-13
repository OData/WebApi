// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DollarFormat
{
    public class DollarFormatWithoutAcceptMediaTypeTests : WebHostTestBase<DollarFormatWithoutAcceptMediaTypeTests>
    {
        public DollarFormatWithoutAcceptMediaTypeTests(WebHostTestFixture<DollarFormatWithoutAcceptMediaTypeTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string, string> BasicMediaTypes
        {
            get
            {
                var data = new TheoryDataSet<string, string>();

                data.Add(Uri.EscapeDataString("json"), "application/json");
                data.Add(Uri.EscapeDataString("application/json"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=false"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=false"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=false"), "application/json");

                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=none"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=none"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=minimal"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=minimal"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=true;odata.metadata=full"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.streaming=false;odata.metadata=full"), "application/json");


                data.Add(Uri.EscapeDataString("Json"), "application/json");
                data.Add(Uri.EscapeDataString("jSoN"), "application/json");
                data.Add(Uri.EscapeDataString("APPLICATION/JSON;ODATA.METADATA=NONE;odata.streaming=TRUE"), "application/json");
                data.Add(Uri.EscapeDataString("aPpLiCaTiOn/JsOn;odata.streaming=tRuE;oDaTa.MeTaDaTa=NoNe"), "application/json");

                return data;
            }
        }

        public static TheoryDataSet<string, string> FeedMediaTypes
        {
            get
            {
                var data = BasicMediaTypes;

                return data;
            }
        }

        public static TheoryDataSet<string, string> EntryMediaTypes
        {
            get
            {
                var data = BasicMediaTypes;

                return data;
            }
        }

        public static TheoryDataSet<string, string> ServiceDocumentMediaTypes
        {
            get
            {
                var data = new TheoryDataSet<string, string>();

                data.Add(Uri.EscapeDataString("json"), "application/json");
                data.Add(Uri.EscapeDataString("application/json"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=none;odata.streaming=false"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=minimal;odata.streaming=false"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=true"), "application/json");
                data.Add(Uri.EscapeDataString("application/json;odata.metadata=full;odata.streaming=false"), "application/json");

                return data;
            }
        }

        public static TheoryDataSet<string, string> MetadataDocumentMediaTypes
        {
            get
            {
                var data = new TheoryDataSet<string, string>();
                data.Add(Uri.EscapeDataString("xml"), "application/xml");
                data.Add(Uri.EscapeDataString("application/xml"), "application/xml");
                return data;
            }
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DollarFormatCustomer>("DollarFormatCustomers");
            builder.EntitySet<DollarFormatOrder>("DollarFormatOrders");

            return builder.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(FeedMediaTypes))]
        public async Task QueryFeedWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string expand = "$expand=SpecialOrder($select=Detail)";
            string filter = "$filter=Id le 5";
            string orderBy = "$orderby=Id desc";
            string select = "$select=Id";
            string format = string.Format("$format={0}", dollarFormat);
            string query = string.Format("?{0}&{1}&{2}&{3}&{4}", expand, filter, orderBy, select, format);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            if (dollarFormat.ToLowerInvariant().Contains("type"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("type"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(EntryMediaTypes))]
        public async Task QueryEntryWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            if (dollarFormat.ToLowerInvariant().Contains("type"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("type"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryPropertyWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=Name&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryNavigationPropertyWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=SpecialOrder&$expand=SpecialOrder&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(BasicMediaTypes))]
        public async Task QueryCollectionWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=Orders&$expand=Orders&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(ServiceDocumentMediaTypes))]
        public async Task QueryServiceDocumentWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.Contains(param.Value, dollarFormat.ToLowerInvariant());
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                XmlReader.Create(await response.Content.ReadAsStreamAsync());
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                await response.Content.ReadAsObject<JObject>();
            }
        }

        [Theory]
        [MemberData(nameof(MetadataDocumentMediaTypes))]
        public async Task QueryMetadataDocumentWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/$metadata" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            XmlReader.Create(await response.Content.ReadAsStreamAsync());
        }
    }
}