// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class SelectImprovementOnComplexTypeTests : WebHostTestBase
    {
        private const string PeopleBaseUrl = "{0}/odata/People";

        public SelectImprovementOnComplexTypeTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(PeopleController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MaxTop(2).Expand().Select().OrderBy().Filter();
            configuration.MapODataServiceRoute("odata", "odata", ModelGenerator.GetConventionalEdmModel());
        }

        #region SubProperty on Single ComplexProperty
        [Theory]
        [InlineData("HomeLocation/Street,HomeLocation/TaxNo")]
        [InlineData("HomeLocation($select=Street,TaxNo)")]
        public void QueryEntityWithSelectOnSubPrimitivePropertyOfComplexProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"HomeLocation\":{\"Street\":\"110th\",\"TaxNo\":19}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Street,HomeLocation/TaxNo)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/Emails")]
        [InlineData("HomeLocation($select=Emails)")]
        public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"HomeLocation\":{\"Emails\":[\"E1\",\"E3\",\"E2\"]}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/RelatedInfo,HomeLocation/AdditionInfos")]
        [InlineData("HomeLocation($select=RelatedInfo,AdditionInfos)")]
        public void QueryEntityWithSelectOnSubComplexPropertyOfComplexProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"HomeLocation\":{\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}," +
                "\"AdditionInfos\":[" +
                  "{\"AreaSize\":102,\"CountyName\":\"King1\"}," +
                  "{\"AreaSize\":103,\"CountyName\":\"King2\"}," +
                  "{\"AreaSize\":104,\"CountyName\":\"King3\"}" +
                "]}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/RelatedInfo,HomeLocation/AdditionInfos)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory(Skip = "Has problem in SelectExpandNode, need to skip the PathSelectItem for Nav?")]
        [InlineData("HomeLocation/ZipCode,HomeLocation/Street")]
        [InlineData("HomeLocation($select=ZipCode,Street)")]
        public void QueryEntityWithSelectOnSubNavigationPropertyOfComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select + "&$format=application/json;odata.metadata=full";

            string value = "\"HomeLocation\":{\"Emails\":[\"E1\",\"E3\",\"E2\"]}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Longitude")]
        [InlineData("HomeLocation($select=Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude,Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Longitude)")]
        public void QueryEntityWithSelectOnDerivedSubPropertyOfComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(2)?$select=" + select;

            string value = "\"HomeLocation\":{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.211\",\"Longitude\":\"231.131\"}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude,HomeLocation/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Longitude)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("OrderInfo/DynamicAddress,OrderInfo/DynamicInt")]
        [InlineData("OrderInfo($select=DynamicAddress,DynamicInt)")]
        public void QueryEntityWithSelectOnSubDynamicPropertyOfComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(5)?$select=" + select;

            string value = "\"OrderInfo\":{" +
                "\"DynamicInt\":9," +
                "\"DynamicAddress\":{" +
                  "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.Address\"," +
                  "\"Street\":\"\"," +
                  "\"TaxNo\":0," +
                  "\"Emails\":[\"abc@1.com\",\"xyz@2.com\"]," +
                  "\"RelatedInfo\":null," +
                  "\"AdditionInfos\":[]" +
                "}}";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(OrderInfo)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(OrderInfo/DynamicAddress,OrderInfo/DynamicInt)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }
        #endregion

        #region SubProperty On Collection ComplexProperty
        [Theory]
        [InlineData("RepoLocations/Street,RepoLocations/TaxNo,RepoLocations/RelatedInfo")]
        [InlineData("RepoLocations($select=Street,TaxNo,RelatedInfo)")]
        public void QueryEntityWithSelectOnSubPropertyOfCollectionComplexProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"RepoLocations\":[" +
                "{\"Street\":\"110th\",\"TaxNo\":19,\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}}," +
                "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"120th\",\"TaxNo\":17,\"RelatedInfo\":null}," +
                "{\"Street\":\"130th\",\"TaxNo\":18,\"RelatedInfo\":{\"AreaSize\":201,\"CountyName\":\"Queue\"}}" +
              "]";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Street,RepoLocations/TaxNo,RepoLocations/RelatedInfo)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations/Emails")]
        [InlineData("RepoLocations($select=Emails)")]
        public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(4)?$select=" + select;

            string value = "\"RepoLocations\":[" +
                "{\"Emails\":[\"E1\",\"E3\",\"E2\"]}," +
                "{" +
                  "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\"," +
                  "\"Emails\":[\"E7\",\"E4\",\"E5\"]" +
                "}" +
              "]";

            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations/RelatedInfo,RepoLocations/AdditionInfos")]
        [InlineData("RepoLocations($select=RelatedInfo,AdditionInfos)")]
        public void QueryEntityWithSelectOnSubComplexPropertyOfCollectionComplexProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(5)?$select=" + select;

            string value = "\"RepoLocations\":[{\"RelatedInfo\":{\"AreaSize\":101,\"CountyName\":\"King\"}," +
                "\"AdditionInfos\":[" +
                  "{\"AreaSize\":102,\"CountyName\":\"King1\"}," +
                  "{\"AreaSize\":103,\"CountyName\":\"King2\"}," +
                  "{\"AreaSize\":104,\"CountyName\":\"King3\"}" +
                "]}]";

            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/RelatedInfo,RepoLocations/AdditionInfos)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude")]
        [InlineData("RepoLocations($select=Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude)")]
        public void QueryEntityWithSelectOnDerivedSubPropertyOfCollectionComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(4)?$select=" + select;

            string value = "\"RepoLocations\":[" +
                  "{}," + // Be noted, this is correct.
                  "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Latitude\":\"12.8\"}" +
                "]";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation/Latitude)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }
        #endregion

        #region Nested query option on select
        [Theory]
        [InlineData("Taxes", "\"Taxes\":[7,5,9]")]
        [InlineData("Taxes($filter=$it eq 5)", "\"Taxes\":[5]")]
        [InlineData("Taxes($filter=$it le 8)", "\"Taxes\":[7,5]")]
        public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedFilter(string select, string value)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;
            string equals = string.Format("{{\"@odata.context\":\"{0}/odata/$metadata#People(Taxes)/$entity\",{1}}}", BaseAddress, value);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("Taxes($orderby=$it)", "\"Taxes\":[5,7,9]")]
        [InlineData("Taxes($orderby =$it desc)", "\"Taxes\":[9,7,5]")]
        public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedOrderby(string select, string value)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;
            string equals = string.Format("{{\"@odata.context\":\"{0}/odata/$metadata#People(Taxes)/$entity\",{1}}}", BaseAddress, value);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("Taxes($top=1;$skip=1)", "\"Taxes\":[5]")]
        [InlineData("Taxes($top=2)", "\"Taxes\":[7,5]")]
        [InlineData("Taxes($top=2;$skip=1)", "\"Taxes\":[5,9]")]
        public void QueryEntityWithSelectOnCollectionPrimitivePropertyWithNestedTopAndSkip(string select, string value)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;
            string equals = string.Format("{{\"@odata.context\":\"{0}/odata/$metadata#People(Taxes)/$entity\",{1}}}", BaseAddress, value);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/Emails($filter=$it eq 'E3')")]
        [InlineData("HomeLocation($select=Emails($filter=$it eq 'E3'))")]
        public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfComplexTypePropertyWithNestedFilter(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," +
                    "\"HomeLocation\":{\"Emails\":[\"E3\"]}}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," +
                    "\"HomeLocation\":{\"Emails\":[\"E3\"]}}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/Emails($orderby=$it)")]
        [InlineData("HomeLocation($select=Emails($orderby=$it desc))")]
        public void QueryEntityWithSelectOnCollectionPrimitivePropertyOfComplexPropertyWithNestedOrderby(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(5)?$select=" + select;

            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)/$entity\"," +
                    "\"HomeLocation\":{\"Emails\":[\"E9\",\"E8\",\"E6\"]}}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Emails)/$entity\"," +
                    "\"HomeLocation\":{\"Emails\":[\"E6\",\"E8\",\"E9\"]}}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("HomeLocation/Emails($top=1;$skip=1)", "\"Emails\":[\"E6\"]")]
        [InlineData("HomeLocation/Emails($top=2)", "\"Emails\":[\"E9\",\"E6\"]")]
        [InlineData("HomeLocation/Emails($top=2;$skip=1)", "\"Emails\":[\"E6\",\"E8\"]")]
        public void QueryEntityWithSelectOnCollectionPrimitivePropertyOfComplexPropertyWithNestedTopAndSkip(string select, string value)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(5)?$select=" + select;
            string equals = string.Format("{{\"@odata.context\":\"{0}/odata/$metadata#People(HomeLocation/Emails)/$entity\",\"HomeLocation\":{{{1}}}}}", BaseAddress, value);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations/Emails($filter=$it eq 'E3')")]
        [InlineData("RepoLocations($select=Emails($filter=$it eq 'E3'))")]
        public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexTypePropertyWithFilter(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"RepoLocations\":[" +
                "{\"Emails\":[\"E3\"]}," +
                "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Emails\":[]}," +
                "{\"Emails\":[]}" +
              "]";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations/Emails($orderby=$it;$top=1;$skip=2)")]
        [InlineData("RepoLocations($select=Emails($orderby=$it;$top=1;$skip=2))")]
        public void QueryEntityWithSelectOnSubCollectionPrimitivePropertyOfCollectionComplexTypePropertyWithOrderByTopSkip(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string value = "\"RepoLocations\":[" +
                "{\"Emails\":[\"E3\"]}," +
                "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Emails\":[\"E7\"]}," +
                "{\"Emails\":[\"E9\"]}" +
              "]";
            string expects;
            if (select.Contains("$select="))
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," + value + "}";
            }
            else
            {
                expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations/Emails)/$entity\"," + value + "}";
            }

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Theory]
        [InlineData("RepoLocations($filter=TaxNo eq 17;$select=AdditionInfos($filter=AreaSize eq 102))")]
        public void QueryEntityWithSelectOnSubCollectionComplexPropertyOfCollectionComplexTypePropertyWithNestedFilter(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "(1)?$select=" + select;

            string expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(RepoLocations)/$entity\"," +
                "\"RepoLocations\":[" +
                  "{" +
                    "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\"," +
                    "\"AdditionInfos\":[" +
                      "{\"AreaSize\":102,\"CountyName\":\"King1\"}" +
                    "]" +
                  "}" +
                "]}";

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }
        #endregion

        [Theory]
        [InlineData("HomeLocation/Street")]
        [InlineData("HomeLocation($select=Street)")]
        public void QueryEntitySetWithSelectOnSubPropertyOfComplexTypeProperty(string select)
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) + "?$select=" + select;

            string value = "\"value\":[" +
                "{\"HomeLocation\":{\"Street\":\"110th\"}}," +
                "{\"HomeLocation\":{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"110th\"}}," +
                "{\"HomeLocation\":{\"Street\":null}}," +
                "{\"HomeLocation\":{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.GeoLocation\",\"Street\":\"120th\"}}," +
                "{\"HomeLocation\":{\"Street\":\"130th\"}}," +
                "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.VipPerson\",\"HomeLocation\":{\"Street\":\"130th\"}}]";

            string expect;
            if (select.Contains("$select="))
            {
                expect = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation)\"," + value + "}";
            }
            else
            {
                expect = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(HomeLocation/Street)\"," + value + "}";
            }

            string equals = expect.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        [Fact]
        public void QueryEntitySetWithSelectOnDerivedProperty()
        {
            // Arrange
            string requestUri = string.Format(PeopleBaseUrl, BaseAddress) +
                "?$select=Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.VipPerson/Bonus";

            string expects = "{\"@odata.context\":\"BASE_ADDRESS/odata/$metadata#People(Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.VipPerson/Bonus)\"," +
                "\"value\":[" +
                "{}," +
                "{}," +
                "{}," +
                "{}," +
                "{}," +
                "{\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType.VipPerson\",\"Bonus\":99}" +
              "]}";

            string equals = expects.Replace("BASE_ADDRESS", BaseAddress);

            // Act & Assert
            ExecuteAndVerifyQueryRequest(requestUri, equals);
        }

        private static string ExecuteAndVerifyQueryRequest(string requestUri, string equals)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpClient client = new HttpClient();

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            string result = response.Content.ReadAsStringAsync().Result;

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(equals, result);

            return result;
        }
    }
}

