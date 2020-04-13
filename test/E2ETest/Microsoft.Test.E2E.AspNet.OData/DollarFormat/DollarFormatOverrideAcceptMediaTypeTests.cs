// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
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
    public class DollarFormatOverrideAcceptMediaTypeTests : WebHostTestBase<DollarFormatOverrideAcceptMediaTypeTests>
    {
        public DollarFormatOverrideAcceptMediaTypeTests(WebHostTestFixture<DollarFormatOverrideAcceptMediaTypeTests> fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string, string, string> BasicMediaTypes
        {
            get
            {
                var data = DollarFormatWithoutAcceptMediaTypeTests.BasicMediaTypes;
                var possibleAcceptMediaTypes = new[]
                    {
                        "application/json",
                        "application/json;odata.metadata=none",
                        "application/json;odata.metadata=minimal",
                        "application/json;odata.metadata=full",
                        "application/json;odata.metadata=none;odata.streaming=true",
                        "application/json;odata.metadata=none;odata.streaming=false",
                        "application/json;odata.metadata=minimal;odata.streaming=true",
                        "application/json;odata.metadata=minimal;odata.streaming=false",
                        "application/json;odata.metadata=full;odata.streaming=true",
                        "application/json;odata.metadata=full;odata.streaming=false",

                        "application/json;odata.streaming=true;odata.metadata=none",
                        "application/json;odata.streaming=false;odata.metadata=none",
                        "application/json;odata.streaming=true;odata.metadata=minimal",
                        "application/json;odata.streaming=false;odata.metadata=minimal",
                        "application/json;odata.streaming=true;odata.metadata=full",
                        "application/json;odata.streaming=false;odata.metadata=full",


                        "APPLICATION/JSON;ODATA.METADATA=NONE;ODATA.streaming=TRUE",
                        "aPpLiCaTiOn/JsOn;oDaTa.streaming=tRuE;oDaTa.MeTaDaTa=NoNe"
                    };
                var basicMediaTypes = new TheoryDataSet<string, string, string>();

                foreach (var datum in data)
                {
                    foreach (var contentType in possibleAcceptMediaTypes)
                    {
                        basicMediaTypes.Add(contentType, (string)datum[0], (string)datum[1]);
                    }
                }

                return basicMediaTypes;
            }
        }

        public static TheoryDataSet<string, string, string> FeedMediaTypes
        {
            get
            {
                TheoryDataSet<string, string> data = DollarFormatWithoutAcceptMediaTypeTests.FeedMediaTypes;

                string[] possibleAcceptMediaTypes = new string[]
                    {
                        "application/json",
                        "application/json;odata.metadata=none",
                        "application/json;odata.metadata=minimal",
                        "application/json;odata.metadata=full",
                        "application/json;odata.metadata=none;odata.streaming=true",
                        "application/json;odata.metadata=none;odata.streaming=false",
                        "application/json;odata.metadata=minimal;odata.streaming=true",
                        "application/json;odata.metadata=minimal;odata.streaming=false",
                        "application/json;odata.metadata=full;odata.streaming=true",
                        "application/json;odata.metadata=full;odata.streaming=false"
                    };

                var feedMediaTypes = new TheoryDataSet<string, string, string>();

                foreach (var datum in data)
                {
                    foreach (var contentType in possibleAcceptMediaTypes)
                    {
                        feedMediaTypes.Add(contentType, (string)datum[0], (string)datum[1]);
                    }
                }

                return feedMediaTypes;
            }
        }

        public static TheoryDataSet<string, string, string> EntryMediaTypes
        {
            get
            {
                var data = DollarFormatWithoutAcceptMediaTypeTests.EntryMediaTypes;
                var possibleAcceptMediaTypes = new[]
                    {
                        "application/json",
                        "application/json;odata.metadata=none",
                        "application/json;odata.metadata=minimal",
                        "application/json;odata.metadata=full",
                        "application/json;odata.metadata=none;odata.streaming=true",
                        "application/json;odata.metadata=none;odata.streaming=false",
                        "application/json;odata.metadata=minimal;odata.streaming=true",
                        "application/json;odata.metadata=minimal;odata.streaming=false",
                        "application/json;odata.metadata=full;odata.streaming=true",
                        "application/json;odata.metadata=full;odata.streaming=false"
                    };
                var entryMediaTypes = new TheoryDataSet<string, string, string>();

                foreach (var datum in data)
                {
                    foreach (var contentType in possibleAcceptMediaTypes)
                    {
                        entryMediaTypes.Add(contentType, (string)datum[0], (string)datum[1]);
                    }
                }

                return entryMediaTypes;
            }
        }

        public static TheoryDataSet<string, string, string> ServiceDocumentMediaTypes
        {
            get
            {
                var data = DollarFormatWithoutAcceptMediaTypeTests.ServiceDocumentMediaTypes;
                var possibleAcceptMediaTypes = new[]
                    {
                        "application/json",
                        "application/json;odata.metadata=none",
                        "application/json;odata.metadata=minimal",
                        "application/json;odata.metadata=full",
                        "application/json;odata.metadata=none;odata.streaming=true",
                        "application/json;odata.metadata=none;odata.streaming=false",
                        "application/json;odata.metadata=minimal;odata.streaming=true",
                        "application/json;odata.metadata=minimal;odata.streaming=false",
                        "application/json;odata.metadata=full;odata.streaming=true",
                        "application/json;odata.metadata=full;odata.streaming=false"
                    };
                var serviceDocumentMediaTypes = new TheoryDataSet<string, string, string>();

                foreach (var datum in data)
                {
                    foreach (var contentType in possibleAcceptMediaTypes)
                    {
                        serviceDocumentMediaTypes.Add(contentType, (string)datum[0], (string)datum[1]);
                    }
                }

                return serviceDocumentMediaTypes;
            }
        }

        public static TheoryDataSet<string, string, string> MetadataDocumentMediaTypes
        {
            get
            {
                var data = DollarFormatWithoutAcceptMediaTypeTests.MetadataDocumentMediaTypes;
                var possibleAcceptMediaTypes = new[]
                    {
                        "application/xml",
                    };
                var metadataDocumentMediaTypes = new TheoryDataSet<string, string, string>();

                foreach (var datum in data)
                {
                    foreach (var contentType in possibleAcceptMediaTypes)
                    {
                        metadataDocumentMediaTypes.Add(contentType, (string)datum[0], (string)datum[1]);
                    }
                }

                return metadataDocumentMediaTypes;
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
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();

            EntitySetConfiguration<DollarFormatCustomer> dollarFormatCustomers =
                builder.EntitySet<DollarFormatCustomer>("DollarFormatCustomers");

            EntitySetConfiguration<DollarFormatOrder> dollarFormatOrders =
                builder.EntitySet<DollarFormatOrder>("DollarFormatOrders");

            return builder.GetEdmModel();
        }

        [Theory]
        [MemberData(nameof(FeedMediaTypes))]
        public async Task QueryFeedWithDollarFormatOverrideAcceptMediaTypeTests(
            string contentType,
            string dollarFormat,
            string expectMediaType)
        {
            string expand = "$expand=SpecialOrder($select=Detail)";
            string filter = "$filter=Id le 5";
            string orderBy = "$orderby=Id desc";
            string select = "$select=Id";
            string format = string.Format("$format={0}", dollarFormat);
            string query = string.Format("?{0}&{1}&{2}&{3}&{4}", expand, filter, orderBy, select, format);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
            if (dollarFormat.ToLowerInvariant().Contains("xml"))
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
        public async Task QueryEntryWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
        public async Task QueryPropertyWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=Name&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
        public async Task QueryNavigationPropertyWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=SpecialOrder&$expand=SpecialOrder&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
        public async Task QueryCollectionWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$select=Orders&$expand=Orders&$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/DollarFormatCustomers(1)" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
        public async Task QueryServiceDocumentWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

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
        public async Task QueryMetadataDocumentWithDollarFormatOverrideAcceptMediaTypeTests(string contentType, string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/$metadata" + query;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(contentType));

            var response = await this.Client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            XmlReader.Create(await response.Content.ReadAsStreamAsync());
        }
    }
}