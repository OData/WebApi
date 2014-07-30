// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using System.Web.OData.Builder;
using System.Web.OData.Builder.Conventions;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Formatter
{
    public class ODataModelBinderProviderTest
    {
        private HttpConfiguration _configuration;
        private HttpServer _server;
        private HttpClient _client;

        public ODataModelBinderProviderTest()
        {
            _configuration = new HttpConfiguration();
            _configuration.Services.Replace(typeof(ModelBinderProvider), new ODataModelBinderProvider());

            _configuration.Routes.MapHttpRoute("default_multiple_keys", "{controller}/{action}({key1}={value1},{key2}={value2})");
            _configuration.Routes.MapHttpRoute("default", "{controller}/{action}({id})");

            _server = new HttpServer(_configuration);
            _client = new HttpClient(_server);
        }

        public static TheoryDataSet<object, string> ODataModelBinderProvider_Works_TestData
        {
            get
            {
                return new TheoryDataSet<object, string>
                {
                    { true, "GetBool" },
                    { (short)123, "GetInt16"},
                    { (short)123, "GetUInt16"},
                    { (int)123, "GetInt32" },
                    { (int)123, "GetUInt32" },
                    { (long)123, "GetInt64" },
                    { (long)123, "GetUInt64" },
                    { (byte)1, "GetByte" },
                    { "123", "GetString" },
                    { Guid.Empty, "GetGuid" },
                    // TODO 1559: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    { TimeSpan.FromTicks(424242), "GetTimeSpan" },
                    { DateTimeOffset.MaxValue, "GetDateTimeOffset" },
                    { float.NaN, "GetFloat" },
                    // TODO 1560: ODataLib v4 issue on decimal handling, bug filed.
                };
            }
        }

        public static TheoryDataSet<object, string> ODataModelBinderProvider_Throws_TestData
        {
            get
            {
                return new TheoryDataSet<object, string>
                {
                    { "123", "GetBool" },
                    // TODO 1559: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    { "abc", "GetInt32" },
                    { "abc", "GetGuid" },
                    { "abc", "GetByte" },
                    { "abc", "GetFloat" },
                    { "abc", "GetDouble" },
                    { "abc", "GetDecimal" },
                    // TODO 1559: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
                    { "abc", "GetTimeSpan" },
                    { "abc", "GetDateTimeOffset" },
                    { -1, "GetUInt16"},
                    { -1, "GetUInt32" },
                    { -1, "GetUInt64"},
                };
            }
        }

        public static TheoryDataSet<string, string, string> ODataModelBinderProvider_ModelStateErrors_InvalidODataRepresentations_TestData
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    { "abc", "GetNullableBool", "Expected literal type token but found token 'abc'." },
                    { "datetime'123'", "GetNullableDateTimeOffset", "Expected literal type token but found token 'datetime'123''." }
                };
            }
        }

        public static TheoryDataSet<string, string, string> ODataModelBinderProvider_ModelStateErrors_InvalidConversions_TestData
        {
            get
            {
                return new TheoryDataSet<string, string, string>
                {
                    { "'abc'", "GetNullableChar", "The value ''abc'' is invalid. The value must be a string with a maximum length of 1." },
                    { "'abc'", "GetDefaultChar", "The value ''abc'' is invalid. The value must be a string with a length of 1." },
                    { "-123", "GetDefaultUInt", "Value was either too large or too small for a UInt32." }
                };
            }
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNull_configuration()
        {
            ODataModelBinderProvider binderProvider = new ODataModelBinderProvider();

            Assert.ThrowsArgumentNull(
                () => binderProvider.GetBinder(configuration: null, modelType: typeof(int)),
                "configuration");
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNull_modelType()
        {
            ODataModelBinderProvider binderProvider = new ODataModelBinderProvider();

            Assert.ThrowsArgumentNull(
                () => binderProvider.GetBinder(new HttpConfiguration(), modelType: null),
                "modelType");
        }

        [Theory]
        [PropertyData("ODataModelBinderProvider_Works_TestData")]
        public void ODataModelBinderProvider_Works(object value, string action)
        {
            string url = String.Format(
                "http://localhost/ODataModelBinderProviderTest/{0}({1})",
                action,
                Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));
            HttpResponseMessage response = _client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            Assert.Equal(
                value,
                response.Content.ReadAsAsync(value.GetType(), _configuration.Formatters).Result);
        }

        [Theory]
        [PropertyData("ODataModelBinderProvider_Throws_TestData")]
        public void ODataModelBinderProvider_Throws(object value, string action)
        {
            string url = String.Format(
                "http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})",
                action,
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4)));
            HttpResponseMessage response = _client.GetAsync(url).Result;

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [ReplaceCulture]
        [PropertyData("ODataModelBinderProvider_ModelStateErrors_InvalidODataRepresentations_TestData")]
        public void ODataModelBinderProvider_ModelStateErrors_InvalidODataRepresentations(string value, string action, string error)
        {
            string url = String.Format("http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})", action, Uri.EscapeDataString(value));
            HttpResponseMessage response = _client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();
            Assert.Equal(
                response.Content.ReadAsAsync<string[]>().Result,
                new[] { error });
        }

        [Theory]
        [ReplaceCulture]
        [PropertyData("ODataModelBinderProvider_ModelStateErrors_InvalidConversions_TestData")]
        public void ODataModelBinderProvider_ModelStateErrors_InvalidConversions(string value, string action, string error)
        {
            string url = String.Format("http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})", action, Uri.EscapeDataString(value));
            HttpResponseMessage response = _client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();
            Assert.Equal(
                response.Content.ReadAsAsync<string[]>().Result,
                new[] { error });
        }

        [Fact]
        public void TestMultipleKeys()
        {
            string url = String.Format(
                "http://localhost/ODataModeBinderMultipleKeys/GetMultipleKeys(name={0},model={1})",
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral("name", ODataVersion.V4)),
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(2009, ODataVersion.V4)));

            HttpResponseMessage response = _client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();
            Assert.Equal(
                "name-2009",
                response.Content.ReadAsAsync<string>().Result);
        }

        [Theory]
        [InlineData(SimpleEnum.First, "GetEnum", "simpleEnum")]
        [InlineData(FlagsEnum.One | FlagsEnum.Two, "GetFlagsEnum", "flagsEnum")]
        [InlineData((SimpleEnum)12, "GetEnum", "simpleEnum")]
        [InlineData((FlagsEnum)23, "GetFlagsEnum", "flagsEnum")]
        public void ODataModelBinderProvider_Works_ForEnum(object value, string action, string parameterName)
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(ModelBinderProvider), new ODataModelBinderProvider());
            configuration.MapODataServiceRoute("odata", "", GetEdmModel());

            var controllers = new[] { typeof(ODataModelBinderProviderTestODataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);

            // Act 
            string url = String.Format(
                "http://localhost/{0}({1}={2})",
                action,
                parameterName,
                Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));
            HttpResponseMessage response = client.GetAsync(url).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(
                value,
                response.Content.ReadAsAsync(value.GetType(), configuration.Formatters).Result);
        }

        [Theory]
        [InlineData("abc", "GetEnum", "simpleEnum")]
        public void ResourceIsNotFound_IfContainsInvalidEnum(object value, string action, string parameterName)
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(ModelBinderProvider), new ODataModelBinderProvider());
            configuration.MapODataServiceRoute("odata", "", GetEdmModel());

            var controllers = new[] { typeof(ODataModelBinderProviderTestODataController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);

            // Act 
            string url = String.Format(
                "http://localhost/{0}({1}={2})",
                action,
                parameterName,
                Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));
            HttpResponseMessage response = client.GetAsync(url).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            FunctionConfiguration getEnum = builder.Function("GetEnum");
            getEnum.Parameter<SimpleEnum>("simpleEnum");
            getEnum.Returns<SimpleEnum>();

            FunctionConfiguration getFlagsEnum = builder.Function("GetFlagsEnum");
            getFlagsEnum.Parameter<FlagsEnum>("flagsEnum");
            getFlagsEnum.Returns<FlagsEnum>();

            return builder.GetEdmModel();
        }
    }

    public class ODataKeyAttribute : ModelBinderAttribute
    {
        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            return new[] { new ODataKeysValueProviderFactory() };
        }

        internal class ODataKeysValueProviderFactory : ValueProviderFactory
        {
            public override IValueProvider GetValueProvider(HttpActionContext actionContext)
            {
                return new ODataKeysValueProvider(actionContext.ControllerContext.RouteData);
            }

            private class ODataKeysValueProvider : IValueProvider
            {
                private IHttpRouteData _routeData;

                public ODataKeysValueProvider(IHttpRouteData routedata)
                {
                    _routeData = routedata;
                }

                public bool ContainsPrefix(string prefix)
                {
                    throw new NotImplementedException();
                }

                public ValueProviderResult GetValue(string key)
                {
                    IEnumerable<KeyValuePair<string, object>> match = _routeData.Values.Where(kvp => kvp.Value.Equals(key) && kvp.Key.StartsWith("key"));
                    if (match.Count() == 1)
                    {
                        KeyValuePair<string, object> data = match.First();
                        int index = Int32.Parse(data.Key.Replace("key", String.Empty));
                        object value = _routeData.Values[String.Format("value{0}", index)];
                        return new ValueProviderResult(value, value.ToString(), CultureInfo.InvariantCulture);
                    }

                    return null;
                }
            }
        }
    }

    public class ODataModelBinderProviderTestController : ApiController
    {
        HttpResponseException _exception = new HttpResponseException(HttpStatusCode.NotImplemented);

        public bool GetBool(bool id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public byte GetByte(byte id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public short GetInt16(short id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public ushort GetUInt16(ushort id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public int GetInt32(int id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public uint GetUInt32(uint id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public long GetInt64(long id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public ulong GetUInt64(ulong id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public string GetString(string id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public Guid GetGuid(Guid id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        // TODO 1559: Investigate how to add support for DataTime in webapi.odata, ODataLib v4 does not support it.
        
        public TimeSpan GetTimeSpan(TimeSpan id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public DateTimeOffset GetDateTimeOffset(DateTimeOffset id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public float GetFloat(float id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public double GetDouble(double id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public decimal GetDecimal(decimal id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        private void ThrowIfInsideThrowsController()
        {
            if (Request.GetRouteData().Values["Controller"].Equals("ODataModelBinderProviderThrowsTest"))
            {
                throw new HttpResponseException(HttpStatusCode.NotImplemented);
            }
        }
    }

    public class ODataModelBinderProviderThrowsTestController : ODataModelBinderProviderTestController
    {
        public IEnumerable<string> GetNullableBool(bool? id)
        {
            return ModelState["id"].Errors.Select(e => e.ErrorMessage);
        }

        public IEnumerable<string> GetNullableDateTimeOffset(DateTimeOffset? id)
        {
            return ModelState["id"].Errors.Select(e => e.ErrorMessage);
        }

        public IEnumerable<string> GetNullableChar(char? id)
        {
            return ModelState["id"].Errors.Select(e => e.ErrorMessage);
        }

        public IEnumerable<string> GetDefaultChar(char id = 'a')
        {
            return ModelState["id"].Errors.Select(e => e.ErrorMessage);
        }

        public IEnumerable<string> GetDefaultUInt(uint id = 0)
        {
            return ModelState["id"].Errors.Select(e => e.Exception.Message);
        }
    }

    public class ODataModeBinderMultipleKeysController : ApiController
    {
        public string GetMultipleKeys([ODataKey]string name, [ODataKey]int model)
        {
            return name + "-" + model;
        }
    }

    public class ODataModelBinderProviderTestODataController : ODataController
    {
        [HttpGet]
        [ODataRoute("GetEnum(simpleEnum={simpleEnum})")]
        public SimpleEnum GetEnum(SimpleEnum simpleEnum)
        {
            return simpleEnum;
        }

        [HttpGet]
        [ODataRoute("GetFlagsEnum(flagsEnum={flagsEnum})")]
        public FlagsEnum GetFlagsEnum(FlagsEnum flagsEnum)
        {
            return flagsEnum;
        }

    }
}
