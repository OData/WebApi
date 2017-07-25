using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using System.Xml;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.DollarFormat
{
    [NuwaFramework]
    public class DollarFormatWithoutAcceptMediaTypeTests
    {
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

        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DollarFormatCustomer>("DollarFormatCustomers");
            builder.EntitySet<DollarFormatOrder>("DollarFormatOrders");

            return builder.GetEdmModel();
        }

        [Theory]
        [PropertyData("FeedMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("EntryMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.metadata"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.metadata"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("BasicMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("BasicMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("BasicMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("ServiceDocumentMediaTypes")]
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
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("odata.streaming"))
            {
                var param = response.Content.Headers.ContentType.Parameters.FirstOrDefault(e => e.Name.Equals("odata.streaming"));
                Assert.NotNull(param);
                Assert.True(dollarFormat.ToLowerInvariant().Contains(param.Value));
            }

            if (dollarFormat.ToLowerInvariant().Contains("atom"))
            {
                ODataHelper.ThrowAtomNotSupported();
            }
            else if (dollarFormat.ToLowerInvariant().Contains("xml"))
            {
                Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
            }
            else if (dollarFormat.ToLowerInvariant().Contains("json"))
            {
                Assert.DoesNotThrow(() => response.Content.ReadAsAsync<JObject>());
            }
        }

        [Theory]
        [PropertyData("MetadataDocumentMediaTypes")]
        public async Task QueryMetadataDocumentWithDollarFormatWithoutAcceptMediaTypeTests(string dollarFormat, string expectMediaType)
        {
            string query = string.Format("?$format={0}", dollarFormat);
            string requestUri = this.BaseAddress + "/odata/$metadata" + query;

            var response = await this.Client.GetAsync(requestUri);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectMediaType, response.Content.Headers.ContentType.MediaType);
            Assert.DoesNotThrow(() => XmlReader.Create(response.Content.ReadAsStreamAsync().Result));
        }
    }
}