using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Extensions;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class PrimitiveTypesMetadataTests
    {
        private HttpClient _client;
        private string _baseAddress;

        public static TheoryDataSet<string, string, string> MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypePropertiesData
        {
            get
            {
                TheoryDataSet<string, string, string> data = new TheoryDataSet<string, string, string>();
                string[] acceptHeaders = new string[] 
                {
                    "application/json;odata=fullmetadata",
                    "application/json;odata=fullmetadata;streaming=true",
                    "application/json;odata=fullmetadata;streaming=false",
                    "application/json;odata=minimalmetadata",
                    "application/json;odata=minimalmetadata;streaming=true",
                    "application/json;odata=minimalmetadata;streaming=false",
                    "application/json;odata=nometadata",
                    "application/json;odata=nometadata;streaming=true",
                    "application/json;odata=nometadata;streaming=false",
                    "application/json",
                    "application/json;streaming=true",
                    "application/json;streaming=false"
                };
                Tuple<string, string>[] propertyNameAndEdmTypes = new Tuple<string, string>[] 
                {
                    Tuple.Create("Id", "Edm.Int32"),
                    Tuple.Create("NullableIntProperty", "Edm.Int32"),
                    Tuple.Create("BinaryProperty", "Edm.Binary"),
                    Tuple.Create("BooleanProperty", "Edm.Boolean"),
                    Tuple.Create("DateTimeProperty", "Edm.DateTime"),
                    Tuple.Create("DecimalProperty", "Edm.Decimal"),
                    Tuple.Create("DoubleProperty", "Edm.Double"),
                    Tuple.Create("SingleProperty", "Edm.Single"),
                    Tuple.Create("GuidProperty", "Edm.Guid"),
                    Tuple.Create("Int16Property", "Edm.Int16"),
                    Tuple.Create("Int32Property", "Edm.Int32"),
                    Tuple.Create("Int64Property", "Edm.Int64"),
                    Tuple.Create("SbyteProperty", "Edm.SByte"),
                    Tuple.Create("DateTimeOffsetProperty", "Edm.DateTimeOffset")
                };
                foreach (var acceptHeader in acceptHeaders)
                {
                    foreach (var propertyName in propertyNameAndEdmTypes)
                    {
                        data.Add(acceptHeader, propertyName.Item1, propertyName.Item2);
                    }
                }
                return data;
            }
        }

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
        public HttpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new ReflectedPropertyRoutingConvention());
            configuration.Routes.MapODataServiceRoute("OData", null, GetEdmModel(configuration), new DefaultODataPathHandler(), conventions);
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<EntityWithSimpleProperties>("EntityWithSimpleProperties");
            return builder.GetEdmModel();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void MetadataIsCorrectForFeedsOfEntriesWithJustPrimitiveTypeProperties(string acceptHeader)
        {
            //Arrange
            EntityWithSimpleProperties[] entities = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/EntityWithSimpleProperties/";
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#EntityWithSimpleProperties";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.SetAcceptHeader(acceptHeader);

            //Act
            var response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.ArrayLength(entities.Length, "value", result);
            if (acceptHeader.Contains("nometadata"))
            {
                JsonAssert.DoesNotContainProperty("odata.metadata", result);
            }
            else
            {
                JsonAssert.Equal(expectedMetadataUrl, "odata.metadata", result);
            }
        }

        [Theory]
        [PropertyData("MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypePropertiesData")]
        public void MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypeProperties(string acceptHeader, string propertyName, string edmType)
        {
            //Arrange
            EntityWithSimpleProperties[] entities = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>();
            EntityWithSimpleProperties entity = entities.First();
            string entryUrl = BaseAddress.ToLowerInvariant() + "/EntityWithSimpleProperties(" + entity.Id + ")/" + propertyName;
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, entryUrl);
            message.SetAcceptHeader(acceptHeader);
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#" + edmType;
            string[] inferableTypes = new string[] { "Edm.Int32", "Edm.Double", "Edm.String", "Edm.Boolean" };
            bool isODataNull = false;

            //Act
            var response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsJObject();
            isODataNull = result.Property("odata.null") != null;

            //Assert
            if (isODataNull)
            {
                expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#Edm.Null";
            }
            if (acceptHeader.Contains("nometadata"))
            {
                if (!isODataNull)
                {
                    JsonAssert.DoesNotContainProperty("odata.*", result);
                }
                else
                {
                    JsonAssert.Equals(true, (bool)result.Property("odata.null"));
                }
            }
            else
            {
                JsonAssert.Equal(expectedMetadataUrl, "odata.metadata", result);
                if (!acceptHeader.Contains("fullmetadata") || (inferableTypes.Contains(edmType) && !IsSpecialValue(result)))
                {
                    JsonAssert.DoesNotContainProperty("odata.type", result);
                }
                else
                {
                    JsonAssert.Equal(edmType, "odata.type", result);
                }
            }
        }

        private static bool IsSpecialValue(JObject value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return value.Properties().Any(p =>
            {
                return p.Name == "value" &&
                (((string)p.Value).Equals("infinity", StringComparison.InvariantCultureIgnoreCase) ||
                ((string)p.Value).Equals("-infinity", StringComparison.InvariantCultureIgnoreCase) ||
                ((string)p.Value).Equals("nan", StringComparison.InvariantCultureIgnoreCase));
            });
        }

        [Theory]
        [InlineData("application/json;odata=fullmetadata")]
        [InlineData("application/json;odata=fullmetadata;streaming=true")]
        [InlineData("application/json;odata=fullmetadata;streaming=false")]
        [InlineData("application/json;odata=minimalmetadata")]
        [InlineData("application/json;odata=minimalmetadata;streaming=true")]
        [InlineData("application/json;odata=minimalmetadata;streaming=false")]
        [InlineData("application/json;odata=nometadata")]
        [InlineData("application/json;odata=nometadata;streaming=true")]
        [InlineData("application/json;odata=nometadata;streaming=false")]
        [InlineData("application/json")]
        [InlineData("application/json;streaming=true")]
        [InlineData("application/json;streaming=false")]
        public void MetadataIsCorrectForAnEntryWithJustPrimitiveTypeProperties(string acceptHeader)
        {
            //Arrange
            EntityWithSimpleProperties[] entities = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>();
            EntityWithSimpleProperties entity = entities.First();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/EntityWithSimpleProperties(" + entity.Id + ")";
            string expectedMetadataUrl = BaseAddress.ToLowerInvariant() + "/$metadata#EntityWithSimpleProperties/@Element";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.SetAcceptHeader(acceptHeader);

            //Act
            HttpResponseMessage response = Client.SendAsync(message).Result;
            JObject result = response.Content.ReadAsJObject();

            //Assert
            JsonAssert.DoesNotContainProperty("BooleanProperty@odata.type", result);
            JsonAssert.DoesNotContainProperty("Id@odata.type", result);
            JsonAssert.DoesNotContainProperty("NullableIntProperty@odata.type", result);
            if (!(((string)result["DoubleProperty"]).Equals("Infinity", StringComparison.InvariantCultureIgnoreCase) ||
                 ((string)result["DoubleProperty"]).Equals("-Infinity", StringComparison.InvariantCultureIgnoreCase) ||
               ((string)result["DoubleProperty"]).Equals("NaN", StringComparison.InvariantCultureIgnoreCase)))
            {
                JsonAssert.DoesNotContainProperty("DoubleProperty@odata.type", result);
            }
            JsonAssert.DoesNotContainProperty("Int32Property@odata.type", result);
            if (acceptHeader.Contains("nometadata"))
            {
                JsonAssert.DoesNotContainProperty("odata.metadata", result);
            }
            else
            {
                JsonAssert.Equal(expectedMetadataUrl, "odata.metadata", result);
            }
            if (acceptHeader.Contains("fullmetadata"))
            {
                JsonAssert.Equal(requestUrl, "odata.id", result);
                JsonAssert.Equal("Edm.Binary", "BinaryProperty@odata.type", result);
                JsonAssert.Equal("Edm.DateTime", "DateTimeProperty@odata.type", result);
                JsonAssert.Equal("Edm.Decimal", "DecimalProperty@odata.type", result);
                JsonAssert.Equal("Edm.Single", "SingleProperty@odata.type", result);
                JsonAssert.Equal("Edm.Guid", "GuidProperty@odata.type", result);
                JsonAssert.Equal("Edm.Int16", "Int16Property@odata.type", result);
                JsonAssert.Equal("Edm.Int64", "Int64Property@odata.type", result);
                JsonAssert.Equal("Edm.SByte", "SbyteProperty@odata.type", result);
                JsonAssert.Equal("Edm.DateTimeOffset", "DateTimeOffsetProperty@odata.type", result);
            }
        }
    }
}
