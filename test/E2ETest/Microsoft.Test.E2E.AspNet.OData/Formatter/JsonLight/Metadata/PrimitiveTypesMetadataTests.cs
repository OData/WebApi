//-----------------------------------------------------------------------------
// <copyright file="PrimitiveTypesMetadataTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    public class PrimitiveTypesMetadataTests : WebHostTestBase
    {
        public PrimitiveTypesMetadataTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            IList<IODataRoutingConvention> conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new ReflectedPropertyRoutingConvention());
            configuration.MapODataServiceRoute("OData", null, GetEdmModel(configuration), new DefaultODataPathHandler(), conventions);
            configuration.AddODataQueryFilter();
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
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
        [MemberData(nameof(AllAcceptHeaders))]
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
            var result = await response.Content.ReadAsObject<JObject>();

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
        [MemberData(nameof(MetadataIsCorrectForThePropertiesOfAnEntryWithJustPrimitiveTypePropertiesData))]
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
            var result = await response.Content.ReadAsObject<JObject>();

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
        [MemberData(nameof(AllAcceptHeaders))]
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
            var result = await response.Content.ReadAsObject<JObject>();

            // Assert
            Assert.Null(result.Property("BooleanProperty@odata.type"));
            Assert.Null(result.Property("Id@odata.type"));
            Assert.Null(result.Property("NullableIntProperty@odata.type"));
            string doublePropertyValue = (string)result["DoubleProperty"];
            if (!doublePropertyValue.Equals("INF", StringComparison.InvariantCulture)
                && !doublePropertyValue.Equals("-INF", StringComparison.InvariantCulture)
                && !doublePropertyValue.Equals("NaN", StringComparison.InvariantCulture))
            {
                Assert.Null(result.Property("DoubleProperty@odata.type"));
            }

            Assert.Null(result.Property("Int32Property@odata.type"));

            if (acceptHeader.Contains("odata.metadata=none"))
            {
                Assert.Null(result.Property("@odata.context"));
            }
            else
            {
                ODataUrlAssert.UrlEquals(expectedContextUrl, result, "@odata.context", BaseAddress);
            }

            if (acceptHeader.Contains("odata.metadata=full"))
            {
                ODataUrlAssert.UrlEquals(requestUrl, result, "@odata.id", BaseAddress);
                Assert.Equal("#Binary", result.Property("BinaryProperty@odata.type").Value);
                Assert.Equal("#Duration", result.Property("DurationProperty@odata.type").Value);
                Assert.Equal("#Decimal", result.Property("DecimalProperty@odata.type").Value);
                Assert.Equal("#Single", result.Property("SingleProperty@odata.type").Value);
                Assert.Equal("#Guid", result.Property("GuidProperty@odata.type").Value);
                Assert.Equal("#Int16", result.Property("Int16Property@odata.type").Value);
                Assert.Equal("#Int64", result.Property("Int64Property@odata.type").Value);
                Assert.Equal("#SByte", result.Property("SbyteProperty@odata.type").Value);
                Assert.Equal("#DateTimeOffset", result.Property("DateTimeOffsetProperty@odata.type").Value);
            }
        }
    }
}
