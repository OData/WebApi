// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;

namespace System.Web.Http.OData.Formatter
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
                    { DateTime.Now, "GetDateTime" },
                    { TimeSpan.FromTicks(424242), "GetTimeSpan" },
                    { DateTimeOffset.MaxValue, "GetDateTimeOffset" },
                    { float.NaN, "GetFloat" },
                    { decimal.MaxValue, "GetDecimal" },
                    // { double.NaN, "GetDouble" } // doesn't work with uri parser.
                    { SimpleEnum.First.ToString(), "GetEnum" },
                    { (FlagsEnum.One | FlagsEnum.Two).ToString(), "GetFlagsEnum" }
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
                    { "abc", "GetEnum" },
                    { "abc", "GetGuid" },
                    { "abc", "GetByte" },
                    { "abc", "GetFloat" },
                    { "abc", "GetDouble" },
                    { "abc", "GetDecimal" },
                    { "abc", "GetDateTime" },
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
                    { "datetime'123'", "GetNullableDateTime", "Unrecognized 'Edm.DateTime' literal 'datetime'123'' at '0' in 'datetime'123''." }
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
            string url = String.Format("http://localhost/ODataModelBinderProviderTest/{0}({1})", action, Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V3)));
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
            string url = String.Format("http://localhost/ODataModelBinderProviderThrowsTest/{0}({1})", action, Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V3)));
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
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral("name", ODataVersion.V3)),
                Uri.EscapeDataString(ODataUriUtils.ConvertToUriLiteral(2009, ODataVersion.V3)));

            HttpResponseMessage response = _client.GetAsync(url).Result;

            response.EnsureSuccessStatusCode();
            Assert.Equal(
                "name-2009",
                response.Content.ReadAsAsync<string>().Result);
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

        public string GetEnum(SimpleEnum id)
        {
            ThrowIfInsideThrowsController();
            return id.ToString();
        }

        public string GetFlagsEnum(FlagsEnum id)
        {
            ThrowIfInsideThrowsController();
            return id.ToString();
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

        public IEnumerable<string> GetNullableDateTime(DateTime? id)
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
}
