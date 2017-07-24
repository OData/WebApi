using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Extensions;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class PrimitiveTypesMetadataTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new ReflectedPropertyRoutingConvention());
            configuration.MapODataServiceRoute("OData", null, GetEdmModel(configuration), new DefaultODataPathHandler(), conventions);
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration config)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder(config);
            builder.EntitySet<EntityWithSimpleProperties>("EntityWithSimpleProperties");
            return builder.GetEdmModel();
        }

        public static TheoryDataSet<string> AllAcceptHeaders
        {
            get
            {
                return ODataAcceptHeaderTestSet.GetInstance().GetAllAcceptHeaders();
            }
        }

        public static TheoryDataSet<string, string, string> MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypePropertiesData
        {
            get
            {
                var data = new TheoryDataSet<string, string, string>();

                var acceptHeaders = new string[] 
                {
                    "application/json;odata.metadata=full",
                    "application/json;odata.metadata=full;odata.streaming=true",
                    "application/json;odata.metadata=full;odata.streaming=false",
                    "application/json;odata.metadata=minimal",
                    "application/json;odata.metadata=minimal;odata.streaming=true",
                    "application/json;odata.metadata=minimal;odata.streaming=false",
                    "application/json;odata.metadata=none",
                    "application/json;odata.metadata=none;odata.streaming=true",
                    "application/json;odata.metadata=none;odata.streaming=false",
                    "application/json",
                    "application/json;odata.streaming=true",
                    "application/json;odata.streaming=false"
                };

                var propertyNameAndEdmTypes = new Tuple<string, string>[] 
                {
                    Tuple.Create("Id", "Edm.Int32"),
                    Tuple.Create("BinaryProperty", "Edm.Binary"),
                    Tuple.Create("BooleanProperty", "Edm.Boolean"),
                    Tuple.Create("DurationProperty", "Edm.Duration"),
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

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task MetadataIsCorrectForFeedsOfEntriesWithJustPrimitiveTypeProperties(
            string acceptHeader)
        {
            // Arrange
            EntityWithSimpleProperties[] entities = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>();
            string requestUrl = BaseAddress.ToLowerInvariant() + "/EntityWithSimpleProperties/";
            string expectedContextUrl = BaseAddress.ToLowerInvariant() + "/$metadata#EntityWithSimpleProperties";
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            message.SetAcceptHeader(acceptHeader);

            // Act
            var response = await Client.SendAsync(message);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            JsonAssert.ArrayLength(entities.Length, "value", result);
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty("@odata.context", result);
            }
            else
            {
                JsonAssert.PropertyEquals(expectedContextUrl, "@odata.context", result);
            }
        }

        [Theory]
        [PropertyData("MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypePropertiesData")]
        public async Task MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypeProperties(
            string acceptHeader,
            string propertyName,
            string edmType)
        {
            // Arrange
            EntityWithSimpleProperties entity = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>()
                                                                   .FirstOrDefault();
            Assert.NotNull(entity);

            string expectedContextUrl = BaseAddress + "/$metadata#EntityWithSimpleProperties(" + entity.Id + ")/" + propertyName;
            string[] inferableTypes = new string[] { "Edm.Int32", "Edm.Double", "Edm.String", "Edm.Boolean" };

            // Act
            var entryUrl = BaseAddress + "/EntityWithSimpleProperties(" + entity.Id + ")/" + propertyName;
            var response = await Client.GetWithAcceptAsync(entryUrl, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty("@odata.*", result);
            }
            else
            {
                ODataUrlAssert.UrlEquals(expectedContextUrl, result, "@odata.context", BaseAddress);
                if (!acceptHeader.Contains("odata.metadata=full") ||
                    (inferableTypes.Contains(edmType) && !result.IsSpecialValue()))
                {
                    JsonAssert.DoesNotContainProperty("@odata.type", result);
                }
                else
                {
                    var unqualifiedTypename = "#" + edmType.Substring(4);
                    JsonAssert.PropertyEquals(unqualifiedTypename, "@odata.type", result);
                }
            }
        }

        [Theory]
        [PropertyData("AllAcceptHeaders")]
        public async Task MetadataIsCorrectForAnEntryWithJustPrimitiveTypeProperties(string acceptHeader)
        {
            // Arrange
            EntityWithSimpleProperties entity = MetadataTestHelpers.CreateInstances<EntityWithSimpleProperties[]>()
                                                                   .FirstOrDefault();
            Assert.NotNull(entity);

            string requestUrl = BaseAddress + "/EntityWithSimpleProperties(" + entity.Id + ")";
            string expectedContextUrl = BaseAddress.ToLowerInvariant() + "/$metadata#EntityWithSimpleProperties/$entity";

            // Act
            var response = await Client.GetWithAcceptAsync(requestUrl, acceptHeader);
            var result = await response.Content.ReadAsAsync<JObject>();

            // Assert
            JsonAssert.DoesNotContainProperty("BooleanProperty@odata.type", result);
            JsonAssert.DoesNotContainProperty("Id@odata.type", result);
            JsonAssert.DoesNotContainProperty("NullableIntProperty@odata.type", result);
            string doublePropertyValue = (string)result["DoubleProperty"];
            if (!doublePropertyValue.Equals("INF", StringComparison.InvariantCulture)
                && !doublePropertyValue.Equals("-INF", StringComparison.InvariantCulture)
                && !doublePropertyValue.Equals("NaN", StringComparison.InvariantCulture))
            {
                JsonAssert.DoesNotContainProperty("DoubleProperty@odata.type", result);
            }

            JsonAssert.DoesNotContainProperty("Int32Property@odata.type", result);

            if (acceptHeader.Contains("odata.metadata=none"))
            {
                JsonAssert.DoesNotContainProperty("@odata.context", result);
            }
            else
            {
                ODataUrlAssert.UrlEquals(expectedContextUrl, result, "@odata.context", BaseAddress);
            }

            if (acceptHeader.Contains("odata.metadata=full"))
            {
                ODataUrlAssert.UrlEquals(requestUrl, result, "@odata.id", BaseAddress);
                JsonAssert.PropertyEquals("#Binary", "BinaryProperty@odata.type", result);
                JsonAssert.PropertyEquals("#Duration", "DurationProperty@odata.type", result);
                JsonAssert.PropertyEquals("#Decimal", "DecimalProperty@odata.type", result);
                JsonAssert.PropertyEquals("#Single", "SingleProperty@odata.type", result);
                JsonAssert.PropertyEquals("#Guid", "GuidProperty@odata.type", result);
                JsonAssert.PropertyEquals("#Int16", "Int16Property@odata.type", result);
                JsonAssert.PropertyEquals("#Int64", "Int64Property@odata.type", result);
                JsonAssert.PropertyEquals("#SByte", "SbyteProperty@odata.type", result);
                JsonAssert.PropertyEquals("#DateTimeOffset", "DateTimeOffsetProperty@odata.type", result);
            }
        }
    }
}
