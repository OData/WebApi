using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter.Extensibility
{
    public class EntityWithPrimitiveAndBinaryProperty
    {
        public int Id { get; set; }
        public long LongProperty { get; set; }
        public byte[] BinaryProperty { get; set; }
        public long? NullableLongProperty { get; set; }
    }

    public class EntityWithPrimitiveAndBinaryPropertyController : EntitySetController<ParentEntity, int>
    {
        private static readonly EntityWithPrimitiveAndBinaryProperty ENTITY;

        static EntityWithPrimitiveAndBinaryPropertyController()
        {
            ENTITY = new EntityWithPrimitiveAndBinaryProperty
            {
                Id = 1,
                LongProperty = long.MaxValue,
                BinaryProperty = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray(),
                NullableLongProperty = null
            };
        }

        public long GetLongProperty(int key)
        {
            return ENTITY.LongProperty;
        }

        public byte[] GetBinaryProperty(int key)
        {
            return ENTITY.BinaryProperty;
        }

        public long? GetNullableLongProperty(int key)
        {
            return ENTITY.NullableLongProperty;
        }
    }

    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class SupportDollarValueTest
    {
        private string _baseAddress;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get { return _baseAddress; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.MapODataServiceRoute("RawValue", "RawValue", GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            //config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(configuration);
            var parentSet = builder.EntitySet<EntityWithPrimitiveAndBinaryProperty>("EntityWithPrimitiveAndBinaryProperty");
            return builder.GetEdmModel();
        }

        [Fact]
        public void CanExtendTheFormatterToSupportPrimitiveRawValues()
        {
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/LongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = Client.SendAsync(message).Result;
            long result = long.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void CanExtendTheFormatterToSupportBinaryRawValues()
        {
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/BinaryProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = Client.SendAsync(message).Result;
            byte[] result = response.Content.ReadAsByteArrayAsync().Result;
        }

        [Fact]
        public void CanExtendTheFormatterToSupportNullRawValues()
        {
            string requestUrl = BaseAddress + "/RawValue/EntityWithPrimitiveAndBinaryProperty(1)/NullableLongProperty/$value";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = Client.SendAsync(message).Result;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
