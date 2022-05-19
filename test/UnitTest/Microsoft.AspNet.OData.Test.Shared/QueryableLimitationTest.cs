//-----------------------------------------------------------------------------
// <copyright file="QueryableLimitationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class QueryableLimitationTest
    {
        private const string BaseAddress = @"http://localhost";
        private HttpClient _client;
        private IEdmModel _model;

        public QueryableLimitationTest()
        {
            _model = GetEdmModel();
            var controllers = new[]
            {
                typeof(QueryLimitCustomersController),
                typeof(OpenCustomersController),
                typeof(MetadataController)
            };
            var server = TestServerFactory.Create(controllers, config =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", _model);
            });
            _client = TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            EntitySetConfiguration<QueryLimitCustomer> customers = builder.EntitySet<QueryLimitCustomer>("QueryLimitCustomers");
            EntitySetConfiguration<QueryLimitOrder> orders = builder.EntitySet<QueryLimitOrder>("QueryLimitOrders");

            // Can limit sorting and filtering primitive properties
            customers.EntityType.Property(p => p.Name).IsNotFilterable().IsNotSortable();
            customers.EntityType.CollectionProperty(p => p.Addresses).IsNotCountable();

            // Can override the behavior specified by the attributes for primitive properties
            customers.EntityType.Property(p => p.Age).IsFilterable().IsSortable();
            customers.EntityType.CollectionProperty(p => p.Numbers).IsCountable();

            // Can limit on relationships
            customers.EntityType.HasMany(c => c.Orders).IsNotNavigable().IsNotExpandable().IsNotCountable();

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task QueryableLimitation_ExposedAsQueryCapabilitesVocabularyAnnotations_InMetadataDocument()
        {
            // Arrange
            string expect = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""Microsoft.AspNet.OData.Test"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""QueryLimitCustomer"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Name"" Type=""Edm.String"" />
        <Property Name=""Addresses"" Type=""Collection(Edm.String)"" />
        <Property Name=""Age"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""Numbers"" Type=""Collection(Edm.Int32)"" Nullable=""false"" />
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""NotFilterableNotSortableLastName"" Type=""Edm.String"" />
        <Property Name=""NonFilterableUnsortableLastName"" Type=""Edm.String"" />
        <Property Name=""Address"" Type=""Edm.String"" />
        <Property Name=""Notes"" Type=""Collection(Edm.String)"" />
        <NavigationProperty Name=""Orders"" Type=""Collection(Microsoft.AspNet.OData.Test.QueryLimitOrder)"" />
        <NavigationProperty Name=""ImportantOrders"" Type=""Collection(Microsoft.AspNet.OData.Test.QueryLimitOrder)"" />
      </EntityType>
      <EntityType Name=""QueryLimitOrder"">
        <Key>
          <PropertyRef Name=""Id"" />
        </Key>
        <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
        <Property Name=""OrderName"" Type=""Edm.String"" />
        <Property Name=""OrderValue"" Type=""Edm.Decimal"" Nullable=""false"" Scale=""Variable"" />
      </EntityType>
      <EntityType Name=""DerivedQueryLimitCustomer"" BaseType=""Microsoft.AspNet.OData.Test.QueryLimitCustomer"">
        <Property Name=""DerivedName"" Type=""Edm.String"" />
      </EntityType>
    </Schema>
    <Schema Namespace=""Default"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityContainer Name=""Container"">
        <EntitySet Name=""QueryLimitCustomers"" EntityType=""Microsoft.AspNet.OData.Test.QueryLimitCustomer"">
          <NavigationPropertyBinding Path=""ImportantOrders"" Target=""QueryLimitOrders"" />
          <NavigationPropertyBinding Path=""Orders"" Target=""QueryLimitOrders"" />
          <Annotation Term=""Org.OData.Capabilities.V1.CountRestrictions"">
            <Record>
              <PropertyValue Property=""Countable"" Bool=""true"" />
              <PropertyValue Property=""NonCountableProperties"">
                <Collection>
                  <PropertyPath>Addresses</PropertyPath>
                </Collection>
              </PropertyValue>
              <PropertyValue Property=""NonCountableNavigationProperties"">
                <Collection>
                  <NavigationPropertyPath>Orders</NavigationPropertyPath>
                  <NavigationPropertyPath>ImportantOrders</NavigationPropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term=""Org.OData.Capabilities.V1.NavigationRestrictions"">
            <Record>
              <PropertyValue Property=""Navigability"">
                <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
              </PropertyValue>
              <PropertyValue Property=""RestrictedProperties"">
                <Collection>
                  <Record>
                    <PropertyValue Property=""NavigationProperty"" NavigationPropertyPath=""Orders"" />
                    <PropertyValue Property=""Navigability"">
                      <EnumMember>Org.OData.Capabilities.V1.NavigationType/Recursive</EnumMember>
                    </PropertyValue>
                  </Record>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term=""Org.OData.Capabilities.V1.FilterRestrictions"">
            <Record>
              <PropertyValue Property=""Filterable"" Bool=""true"" />
              <PropertyValue Property=""RequiresFilter"" Bool=""true"" />
              <PropertyValue Property=""RequiredProperties"">
                <Collection />
              </PropertyValue>
              <PropertyValue Property=""NonFilterableProperties"">
                <Collection>
                  <PropertyPath>Name</PropertyPath>
                  <PropertyPath>Orders</PropertyPath>
                  <PropertyPath>NotFilterableNotSortableLastName</PropertyPath>
                  <PropertyPath>NonFilterableUnsortableLastName</PropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term=""Org.OData.Capabilities.V1.SortRestrictions"">
            <Record>
              <PropertyValue Property=""Sortable"" Bool=""true"" />
              <PropertyValue Property=""AscendingOnlyProperties"">
                <Collection />
              </PropertyValue>
              <PropertyValue Property=""DescendingOnlyProperties"">
                <Collection />
              </PropertyValue>
              <PropertyValue Property=""NonSortableProperties"">
                <Collection>
                  <PropertyPath>Name</PropertyPath>
                  <PropertyPath>Orders</PropertyPath>
                  <PropertyPath>NotFilterableNotSortableLastName</PropertyPath>
                  <PropertyPath>NonFilterableUnsortableLastName</PropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
          <Annotation Term=""Org.OData.Capabilities.V1.ExpandRestrictions"">
            <Record>
              <PropertyValue Property=""Expandable"" Bool=""true"" />
              <PropertyValue Property=""NonExpandableProperties"">
                <Collection>
                  <NavigationPropertyPath>Orders</NavigationPropertyPath>
                </Collection>
              </PropertyValue>
            </Record>
          </Annotation>
        </EntitySet>
        <EntitySet Name=""QueryLimitOrders"" EntityType=""Microsoft.AspNet.OData.Test.QueryLimitOrder"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*<", @"<");

            string requestUri = BaseAddress + "/odata/$metadata";

            // Act
            HttpResponseMessage response = await _client.GetAsync(requestUri);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expect, responseString);
        }

        [Fact]
        public async Task QueryableLimitation_NotSortableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$orderby=Name";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Name' cannot be used in the $orderby query option.",
                responseString);
        }

        [Theory]
        [InlineData("NotFilterableNotSortableLastName")]
        [InlineData("NonFilterableUnsortableLastName")]
        public async Task QueryableLimitation_NotSortableFromAttributeTest(string property)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$orderby=" + property;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(
                String.Format("The query specified in the URI is not valid. The property '{0}' cannot be used in the $orderby query option.", property),
                responseString);
        }

        [Fact]
        public async Task QueryableLimitation_NotFilterableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=Name eq 'FirstName 1'";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Name' cannot be used in the $filter query option.",
                responseString);
        }

        [Theory]
        [InlineData("NotFilterableNotSortableLastName")]
        [InlineData("NonFilterableUnsortableLastName")]
        public async Task QueryableLimitation_NotFilterableFromAttributeTest(string property)
        {
            // Arrange
            string requestUri = BaseAddress +
                String.Format("/odata/QueryLimitCustomers?$filter={0} eq 'LastName 1'", property);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(
                String.Format(
                    "The query specified in the URI is not valid. The property '{0}' cannot be used in the $filter query option.",
                    property),
                responseString);
        }

        [Fact]
        public async Task QueryableLimitation_NotFilterableAttributeOverrideByModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=Age eq 31";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"Age\":31", responseString);
        }

        [Fact]
        public async Task QueryableLimitation_NotNavigableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$select=Orders";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Orders' cannot be used for navigation.",
                responseString);
        }

        [Fact]
        public async Task QueryableLimitation_NotExpandableFromModelTest()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$expand=Orders";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Orders' cannot be used in the $expand query option.",
                responseString);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Addresses?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Addresses/$count")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Addresses?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Addresses/$count")]
        public async Task QueryableLimitation_NotCountableFromModelTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'Addresses' cannot be used for $count.",
                responseString);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/ImportantOrders?$count=true")]
        [InlineData("QueryLimitCustomers(1)/ImportantOrders/$count")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/ImportantOrders?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/ImportantOrders/$count")]
        public async Task QueryableLimitation_NotCountableFromAttributeTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The query specified in the URI is not valid. The property 'ImportantOrders' cannot be used for $count.",
                responseString);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Numbers?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Numbers/$count")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Numbers?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Numbers/$count")]
        public async Task QueryableLimitation_NotCountableAttributeOverrideByModelTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("QueryLimitCustomers(1)/Notes?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Notes/$count")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Notes?$count=true")]
        [InlineData("QueryLimitCustomers(1)/Microsoft.AspNet.OData.Test.DerivedQueryLimitCustomer/Notes/$count")]
        public async Task QueryableLimitation_CountNotAllowedInQueryOptionsTest(string uri)
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/" + uri;

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(
                "The query specified in the URI is not valid. Query option 'Count' is not allowed. To allow it, " +
                "set the 'AllowedQueryOptions' property on EnableQueryAttribute or QueryValidationSettings.",
                responseString);
        }

        [Fact]      
        public async Task QueryableLimitation_WithConcurrentRequests_AnyAllowedInFilter()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/QueryLimitCustomers?$filter=ImportantOrders/any()";
            //first request to initialize the service
            await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

            // Act            
            var requests = Enumerable.Range(0, 200)
                                     .Select(_ => Task.Run(() => _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri))))
                                     .ToArray();
            HttpResponseMessage[] responses = await Task.WhenAll(requests);

            // Assert
            Assert.True(responses.All(r => r.IsSuccessStatusCode));
            Assert.True(responses.All(r => r.StatusCode == HttpStatusCode.OK));
        }

        // Controller
        public class QueryLimitCustomersController : TestODataController
        {
            private IList<QueryLimitCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                    new QueryLimitCustomer
                    {
                        Id = i,
                        Name = "FirstName " + i,
                        NotFilterableNotSortableLastName = "NotFilterableNotSortableLastName " + i,
                        NonFilterableUnsortableLastName = "NonFilterableUnsortableLastName " + i,
                        Age = 30 + i,
                        Address = "Address " + i,
                        Addresses = new[] { "Address " + i },
                        Numbers = new[] { i },
                        Notes = new[] { "Note " + i },
                        Orders = Enumerable.Range(0, i).Select(j =>
                            new QueryLimitOrder
                            {
                                Id = j,
                                OrderName = "Order_" + i + "_" + j,
                                OrderValue = j
                            }).ToList(),
                        ImportantOrders = Enumerable.Range(0, i).Select(j =>
                            new QueryLimitOrder
                            {
                                Id = j,
                                OrderName = "Order_" + i + "_" + j,
                                OrderValue = j
                            }).ToList()
                    }).ToList();

            [EnableQuery(PageSize = 10, MaxExpansionDepth = 5, MaxAnyAllExpressionDepth = 1)]
            public ITestActionResult Get()
            {
                return Ok(customers);
            }

            [EnableQuery]
            public ITestActionResult GetAddresses(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Addresses);
            }

            [EnableQuery]
            public ITestActionResult GetNumbers(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Numbers);
            }

            [EnableQuery]
            public ITestActionResult GetImportantOrders(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).ImportantOrders);
            }

            [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All ^ AllowedQueryOptions.Count)]
            public ITestActionResult GetNotes(int key)
            {
                return Ok(customers.Single(customer => customer.Id == key).Notes);
            }
        }

        // Models
        public class QueryLimitCustomer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            [NotFilterable]
            [NotSortable]
            public string NotFilterableNotSortableLastName { get; set; }

            [NonFilterable]
            [Unsortable]
            public string NonFilterableUnsortableLastName { get; set; }

            [NotFilterable]
            [NotSortable]
            public int Age { get; set; }

            [NotNavigable]
            public string Address { get; set; }

            public IEnumerable<string> Addresses { get; set; }

            [NotCountable]
            public int[] Numbers { get; set; }

            public string[] Notes { get; set; }

            public ICollection<QueryLimitOrder> Orders { get; set; }

            [NotCountable]
            public IList<QueryLimitOrder> ImportantOrders { get; set; }
        }

        public class QueryLimitOrder
        {
            public int Id { get; set; }
            public string OrderName { get; set; }
            public decimal OrderValue { get; set; }
        }

        public class DerivedQueryLimitCustomer : QueryLimitCustomer
        {
            public string DerivedName { get; set; }
        }
    }
}
