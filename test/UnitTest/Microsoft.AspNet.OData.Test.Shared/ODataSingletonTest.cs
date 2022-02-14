//-----------------------------------------------------------------------------
// <copyright file="ODataSingletonTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class ODataSingletonTest
    {
        private const string BaseAddress = @"http://localhost";
        private HttpClient _client;

        public ODataSingletonTest()
        {
            var controllers = new[] { typeof(OscorpController), typeof(OscorpSubsController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });

            _client = TestServerFactory.CreateClient(server);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Corporation>("Oscorp");
            builder.EntitySet<Subsidiary>("OscorpSubs");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ODataSingleton_WorksOnSingleton_WithFullMetadatas()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/Oscorp";

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = await _client.SendAsync(request);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("http://localhost/odata/$metadata#Oscorp", (string)result["@odata.context"]);
            Assert.Equal("#Microsoft.AspNet.OData.Test.Corporation", (string)result["@odata.type"]);
            Assert.Equal("http://localhost/odata/Oscorp", (string)result["@odata.id"]);
            Assert.Equal("http://localhost/odata/Oscorp/SubSidiaries", (string)result["SubSidiaries@odata.navigationLink"]);
            VerifySingleton(result);
        }

        public static TheoryDataSet<string, string> NavigationPropertyToTest
        {
            get
            {
                return new TheoryDataSet<string, string>
                {
                    {"Oscorp/SubSidiaries", Resources.SingletonNavigationToEntitysetFullMetadata},
                    {"OscorpSubs(101)/HeadQuarter", Resources.EntityNavigationToSingletonFullMetadata}
                };
            }
        }

        [Theory]
        [MemberData(nameof(NavigationPropertyToTest))]
        public async Task ODataSingleton_WorksOnNavigationProperty(string path, string expectedPayload)
        {
            // Remove indentation in expect string 
            expectedPayload = Regex.Replace(expectedPayload, @"\r\n\s*([""{}\]])", "$1");

            // Arrange
            string requestUri = BaseAddress + "/odata/" + path;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

            // Act
            var response = await _client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectedPayload, responseString);
        }

        [Fact]
        public async Task ODataSingleton_WorksOn_EntityReferenceLink()
        {
            // 1. It's successful to query the navigation property
            string requestUri = BaseAddress + "/odata/OscorpSubs(102)/HeadQuarter";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await _client.SendAsync(request);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

            // 2. Use the $ref to delete the reference link.
            requestUri = BaseAddress + "/odata/OscorpSubs(102)/HeadQuarter/$ref";
            request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            response = await _client.SendAsync(request);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);

            // 3. Now, Can't navigation to the navigation property
            requestUri = BaseAddress + "/odata/OscorpSubs(102)/HeadQuarter";
            request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            response = await _client.SendAsync(request);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        private void VerifySingleton(JObject result)
        {
            Assert.Equal(9999, result["CorpId"]);
            Assert.Equal("Oscorp Enterprise", result["CorpName"]);
            Assert.Equal("www.Oscorp.com", result["CorpWeb"]);
            Assert.Equal("Oscorp Way #001", result["CorpAddress"]);
        }

        // Controllers
        public class OscorpController : TestODataController
        {
            public ITestActionResult Get()
            {
                return Ok(ModelBase.Oscorp);
            }

            public ITestActionResult GetSubSidiaries()
            {
                return Ok(ModelBase.Oscorp.SubSidiaries);
            }
        }

        public class OscorpSubsController : TestODataController
        {
            public ITestActionResult GetHeadQuarter(int key)
            {
                Subsidiary sub = ModelBase.Oscorp.SubSidiaries.SingleOrDefault(s => s.SubId == key);
                if (sub.HeadQuarter == null)
                {
                    return NotFound();
                }

                return Ok(sub.HeadQuarter);
            }

            public ITestActionResult DeleteRef(int key, string navigationProperty)
            {
                if (navigationProperty != "HeadQuarter")
                { 
                    return NotFound();
                }

                Subsidiary sub = ModelBase.Oscorp.SubSidiaries.SingleOrDefault(s => s.SubId == key);
                sub.HeadQuarter = null;
                return Ok();
            }
        }

        // Models
        private static class ModelBase
        {
            private static Corporation _oscorp;
            public static Corporation Oscorp
            {
                get
                {
                    if (_oscorp == null)
                    {
                        _oscorp = new Corporation
                        {
                            CorpId = 9999,
                            CorpName = "Oscorp Enterprise",
                            CorpWeb = "www.Oscorp.com",
                            CorpAddress = "Oscorp Way #001",
                            SubSidiaries = Enumerable.Range(0, 10).Select(j =>
                                new Subsidiary
                                {
                                    SubId = 100 + j,
                                    SubName = "Oscorp Subsidiary #" + j,
                                    SubValue = 9999 * (j + 1),
                                    HeadQuarter = null
                                }).ToList()
                        };

                        foreach (var sub in _oscorp.SubSidiaries)
                        {
                            sub.HeadQuarter = _oscorp;
                        }
                    }
                    return _oscorp;
                }
            }
        }

        private sealed class Corporation
        {
            [Key]
            public int CorpId { get; set; }
            public string CorpName { get; set; }
            public string CorpWeb { get; set; }
            public string CorpAddress { get; set; }
            public ICollection<Subsidiary> SubSidiaries { get; set; }
        }

        private sealed class Subsidiary
        {
            [Key]
            public int SubId { get; set; }
            public string SubName { get; set; }
            public decimal SubValue { get; set; }
            [Singleton]
            public Corporation HeadQuarter { get; set; }
        }
    }
}
