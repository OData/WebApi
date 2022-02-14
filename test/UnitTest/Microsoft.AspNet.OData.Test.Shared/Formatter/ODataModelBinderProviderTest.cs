//-----------------------------------------------------------------------------
// <copyright file="ODataModelBinderProviderTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Builder.Conventions;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
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
                    { TimeSpan.FromTicks(424242), "GetTimeSpan" },
                    { DateTimeOffset.MaxValue, "GetDateTimeOffset" },
                    { Date.MaxValue, "GetDate" },
                    { TimeOfDay.MinValue, "GetTimeOfDay" },
                    { decimal.MaxValue, "GetDecimal" },
                    { float.NaN, "GetFloat" },
                    { double.NaN, "GetDouble" }
                };
            }
        }

        public static TheoryDataSet<object, Type, string> ODataModelBinderProvider_Works_NullableTestData
        {
            get
            {
                return new TheoryDataSet<object, Type, string>
                {
                    { Date.MaxValue, typeof(Date?), "GetNullableDate" },
                    { null, typeof(Date?), "GetNullableDate" },
                    { TimeOfDay.MaxValue, typeof(TimeOfDay?), "GetNullableTimeOfDay" },
                    { null, typeof(TimeOfDay?), "GetNullableTimeOfDay" },
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
                    { 123, "GetDateTime" },
                    { "abc", "GetInt32" },
                    { "abc", "GetGuid" },
                    { "abc", "GetByte" },
                    { "abc", "GetFloat" },
                    { "abc", "GetDouble" },
                    { "abc", "GetDecimal" },
                    { "abc", "GetDateTime" },
                    { "abc", "GetTimeSpan" },
                    { "abc", "GetDateTimeOffset" },
                    { "abc", "GetDate" },
                    { "abc", "GetTimeOfDay" },
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

            ExceptionAssert.ThrowsArgumentNull(
                () => binderProvider.GetBinder(configuration: null, modelType: typeof(int)),
                "configuration");
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNull_modelType()
        {
            ODataModelBinderProvider binderProvider = new ODataModelBinderProvider();

            ExceptionAssert.ThrowsArgumentNull(
                () => binderProvider.GetBinder(new HttpConfiguration(), modelType: null),
                "modelType");
        }

        [Theory]
        [MemberData(nameof(ODataModelBinderProvider_Works_TestData))]
        public async Task ODataModelBinderProvider_Works(object value, string action)
        {
            // Arrange
            string url = String.Format("http://localhost/ODataModelBinderProviderTest/{0}({1})",
                action,
                Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            Assert.Equal(
                value,
                await response.Content.ReadAsAsync(value.GetType(), _configuration.Formatters));
        }

        [Theory]
        [MemberData(nameof(ODataModelBinderProvider_Works_NullableTestData))]
        public async Task ODataModelBinderProvider_Works_ForNullable(object value, Type type, string action)
        {
            // Arrange
            string url = String.Format("http://localhost/ODataModelBinderProviderTest/{0}({1})", action,
                value == null ? "null" : Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));

            // Act
            HttpResponseMessage response = await _client.GetAsync(url);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            Assert.Equal(
                value,
                await response.Content.ReadAsAsync(type, _configuration.Formatters));
        }

        [Fact]
        public async Task ODataModelBinderProvider_Works_DateTime()
        {
            TimeZoneInfoHelper.TimeZone = null;
            DateTime value = new DateTime(2014, 11, 5, 0, 0, 0, DateTimeKind.Local);
            string url = String.Format(
                "http://localhost/ODataModelBinderProviderTest/GetDateTime({0})",
                Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));
            HttpResponseMessage response = await _client.GetAsync(url);
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                value,
                await response.Content.ReadAsAsync(value.GetType(), _configuration.Formatters));
        }

        [Theory]
        [MemberData(nameof(ODataModelBinderProvider_Throws_TestData))]
        public async Task ODataModelBinderProvider_Throws(object value, string action)
        {
            string url = String.Format(
                "http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})",
                action,
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V4)));
            HttpResponseMessage response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [ReplaceCulture]
        [MemberData(nameof(ODataModelBinderProvider_ModelStateErrors_InvalidODataRepresentations_TestData))]
        public async Task ODataModelBinderProvider_ModelStateErrors_InvalidODataRepresentations(string value, string action, string error)
        {
            string url = String.Format("http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})", action, Uri.EscapeDataString(value));
            HttpResponseMessage response = await _client.GetAsync(url);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                await response.Content.ReadAsObject<string[]>(),
                new[] { error });
        }

        [Theory]
        [ReplaceCulture]
        [MemberData(nameof(ODataModelBinderProvider_ModelStateErrors_InvalidConversions_TestData))]
        public async Task ODataModelBinderProvider_ModelStateErrors_InvalidConversions(string value, string action, string error)
        {
            string url = String.Format("http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})", action, Uri.EscapeDataString(value));
            HttpResponseMessage response = await _client.GetAsync(url);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                await response.Content.ReadAsObject<string[]>(),
                new[] { error });
        }

        [Fact]
        public async Task TestMultipleKeys()
        {
            string url = String.Format(
                "http://localhost/ODataModeBinderMultipleKeys/GetMultipleKeys(name={0},model={1})",
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral("name", ODataVersion.V4)),
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(2009, ODataVersion.V4)));

            HttpResponseMessage response = await _client.GetAsync(url);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                "name-2009",
                await response.Content.ReadAsObject<string>());
        }

        [Theory]
        [InlineData(SimpleEnum.First, "GetEnum", "simpleEnum")]
        [InlineData(FlagsEnum.One | FlagsEnum.Two, "GetFlagsEnum", "flagsEnum")]
        [InlineData((SimpleEnum)12, "GetEnum", "simpleEnum")]
        [InlineData((FlagsEnum)23, "GetFlagsEnum", "flagsEnum")]
        public async Task ODataModelBinderProvider_Works_ForEnum(object value, string action, string parameterName)
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
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                value,
                await response.Content.ReadAsAsync(value.GetType(), configuration.Formatters));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(FlagsEnum.One, true)]
        public async Task ODataModelBinderProvider_Works_ForNullableEnum(object value, bool expect)
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
                "http://localhost/GetNullableFlagsEnum(flagsEnum={0})",
                value == null ? "null" : Uri.EscapeDataString(ConventionsHelpers.GetUriRepresentationForValue(value)));
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            Assert.Equal(
                expect,
                await response.Content.ReadAsAsync(typeof(bool), configuration.Formatters));
        }

        [Theory]
        [InlineData("abc", "GetEnum", "simpleEnum")]
        public async Task ResourceIsNotFound_IfContainsInvalidEnum(object value, string action, string parameterName)
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
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void DefaultODataPathHandler_IfContainsStringAsEnum()
        {
            // Arrange
            string value = "First", action = "GetEnum", parameterName = "simpleEnum";
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("GetAddress", "address", "{\"@odata.type\":\"%23NS.Address\",\"City\":\"Shanghai\"}")]
        [InlineData("GetCustomer", "customer", "{\"@odata.type\":\"%23NS.Customer\",\"Id\":9,\"Name\":\"Robot\"}")]
        public async Task ODataModelBinderProvider_Works_OtherParameters(string action, string parameterName, string parameterValue)
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
            string url = String.Format("http://localhost/{0}({1}=@p)?@p={2}", action, parameterName, parameterValue);
            HttpResponseMessage response = await client.GetAsync(url);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntityType<Customer>().Namespace = "NS";
            builder.ComplexType<Address>().Namespace = "NS";

            FunctionConfiguration getEnum = builder.Function("GetEnum");
            getEnum.Parameter<SimpleEnum>("simpleEnum");
            getEnum.Returns<SimpleEnum>();

            FunctionConfiguration getFlagsEnum = builder.Function("GetFlagsEnum");
            getFlagsEnum.Parameter<FlagsEnum>("flagsEnum");
            getFlagsEnum.Returns<FlagsEnum>();

            FunctionConfiguration function = builder.Function("GetNullableFlagsEnum").Returns<bool>();
            function.Parameter<FlagsEnum?>("flagsEnum");

            builder.Function("GetAddress").Returns<bool>().Parameter<Address>("address");
            builder.Function("GetCustomer").Returns<bool>().Parameter<Customer>("customer");
            return builder.GetEdmModel();
        }

        public class Address
        {
            public string City { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

        public DateTime GetDateTime(DateTime id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

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

        public Date GetDate([FromODataUri]Date id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public Date? GetNullableDate([FromODataUri] Date? id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public TimeOfDay GetTimeOfDay([FromODataUri]TimeOfDay id)
        {
            ThrowIfInsideThrowsController();
            return id;
        }

        public TimeOfDay? GetNullableTimeOfDay([FromODataUri]TimeOfDay? id)
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

        [HttpGet]
        [ODataRoute("GetNullableFlagsEnum(flagsEnum={flagsEnum})")]
        public bool GetNullableFlagsEnum(FlagsEnum? flagsEnum)
        {
            if (flagsEnum != null)
            {
                return true;
            }

            Assert.True(ModelState.IsValid);
            return false;
        }

        [HttpGet]
        [ODataRoute("GetAddress(address={address})")]
        public bool GetAddress([FromODataUri]ODataModelBinderProviderTest.Address address)
        {
            Assert.NotNull(address);
            Assert.Equal("Shanghai", address.City);
            return true;
        }

        [HttpGet]
        [ODataRoute("GetCustomer(customer={customer})")]
        public bool GetCustomer([FromODataUri]ODataModelBinderProviderTest.Customer customer)
        {
            Assert.NotNull(customer);
            Assert.Equal(9, customer.Id);
            Assert.Equal("Robot", customer.Name);
            return true;
        }
    }
}
#endif
