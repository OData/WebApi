//-----------------------------------------------------------------------------
// <copyright file="AutoExpandNavigationOnComplexTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.AutoExpand
{
    public class AutoExpandNavigationOnComplexTests : WebHostTestBase
    {
        public AutoExpandNavigationOnComplexTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.AddControllers(typeof(ManagersController));
            configuration.JsonReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("autoexpand", "autoexpand", GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<Manager>("Managers");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        [Fact]
        public async Task QueryForAnResource_Includes_DerivedAutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Managers(8)", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("{" +
                "\"Id\":8," +
                "\"HomeAddress\":" +
                "{" +
                  "\"Street\":\"CnStreet 8\"," +
                  "\"City\":\"CnCity 8\"," +
                  "\"CountryOrRegion\":" +
                  "{" +
                    "\"Id\":108," +
                    "\"Name\":\"C and R 108\"" +
                  "}" +
                "}," +
                "\"Depart\":{\"Id\":108}" +
              "}", payload);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task QueryForAnResource_Includes_DerivedAutoExpandNavigationProperty_WithAndWithoutExpand(bool hasExpand)
        {
            // Arrange
            string queryUrl;
            if (hasExpand)
            {
                queryUrl = string.Format("{0}/autoexpand/Managers(5)?$expand=Depart", BaseAddress);
            }
            else
            {
                queryUrl = string.Format("{0}/autoexpand/Managers(5)", BaseAddress);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("" +
              "{" +
                "\"Id\":5," +
                "\"HomeAddress\":" +
                "{" +
                  "\"Street\":\"UsStreet 5\"," +
                  "\"City\":\"UsCity 5\"," +
                  "\"CountryOrRegion\":{\"Id\":105,\"Name\":\"C and R 105\"}," +
                  "\"ZipCode\":{\"Id\":2005,\"Code\":\"Code 5\"}" +
                "}," +
                "\"Depart\":{\"Id\":105}" +
              "}", payload);
        }

#if NETCORE
        [Fact]
        public async Task QueryForProperty_Includes_AutoExpandNavigationProperty()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Managers(8)/HomeAddress", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);
            string payload = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

          //  string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("{" +
                "\"Street\":\"CnStreet 8\"," +
                "\"City\":\"CnCity 8\"," +
                "\"CountryOrRegion\":{\"Id\":108,\"Name\":\"C and R 108\"}" +
              "}", payload);
        }
#endif

        [Fact]
        public async Task QueryForProperty_Includes_AutoExpandNavigationPropertyOnDerivedType()
        {
            // Arrange
            string queryUrl = string.Format("{0}/autoexpand/Managers(9)/HomeAddress", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = new HttpClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal("{" +
                "\"Street\":\"UsStreet 9\"," +
                "\"City\":\"UsCity 9\"," +
                "\"CountryOrRegion\":{\"Id\":109,\"Name\":\"C and R 109\"}," +
                "\"ZipCode\":{\"Id\":2009,\"Code\":\"Code 9\"}" +
              "}", payload);
        }
    }

    public class ManagersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        public static IList<Manager> _managers = GenerateCustomers();

        private static IList<Manager> GenerateCustomers()
        {
            IList<Manager> managers = new List<Manager>();
            for (int i = 1; i < 10; i++)
            {
                Address address;
                if (i % 2 == 0)
                {
                    address = new CnAddress
                    {
                        Street = $"CnStreet {i}",
                        City = $"CnCity {i}",
                        CountryOrRegion = new CountryOrRegion { Id = i + 100, Name = $"C and R {i + 100}" },
                        PostCode = new PostCodeInfo { Id = i + 1000, Name = $"PostCode {i}" }
                    };
                }
                else
                {
                    address = new UsAddress
                    {
                        Street = $"UsStreet {i}",
                        City = $"UsCity {i}",
                        CountryOrRegion = new CountryOrRegion { Id = i + 100, Name = $"C and R {i + 100}" },
                        ZipCode = new ZipCodeInfo { Id = i + 2000, Code = $"Code {i}" }
                    };
                }

                var manager = new Manager
                {
                    Id = i,
                    HomeAddress = address,
                    Depart = new Department { Id = i  + 100 }
                };

                managers.Add(manager);
            }

            return managers;
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_managers);
        }

        [EnableQuery]
        public TestSingleResult<Manager> Get(int key)
        {
            return TestSingleResult.Create(_managers.Where(c => c.Id == key).AsQueryable());
        }

        [EnableQuery]
        public ITestActionResult GetHomeAddress(int key)
        {
            Manager c = _managers.FirstOrDefault(m => m.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find manager with key = {key}");
            }

            return Ok(c.HomeAddress);
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }

    [AutoExpand]
    public class Manager
    {
        public int Id { get; set; }

        public Address HomeAddress { get; set; }

        public Department Depart { get; set; }
    }

    public class Department
    {
        public int Id { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        [AutoExpand]
        public CountryOrRegion CountryOrRegion { get; set; }
    }

    public class CnAddress : Address
    {
        public PostCodeInfo PostCode { get; set; }
    }

    public class UsAddress : Address
    {
        [AutoExpand]
        public ZipCodeInfo ZipCode { get; set; }
    }

    public class CountryOrRegion
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class PostCodeInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ZipCodeInfo
    {
        public int Id { get; set; }

        public string Code { get; set; }
    }
}
