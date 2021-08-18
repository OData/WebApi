//-----------------------------------------------------------------------------
// <copyright file="ODataLevelsTest.cs" company=".NET Foundation">
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
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataLevelsTest
    {
        private HttpClient _client;

        public ODataLevelsTest()
        {
            IEdmModel model = GetEdmModel();
            Type[] controllers = new[] { typeof(LevelsEntitiesController) };
            var _nullPrefixServer = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
                config.MapODataServiceRoute("odata", "odata", model);
            });

            _client = TestServerFactory.CreateClient(_nullPrefixServer);
        }

        [Fact]
        public async Task Levels_ExpandsNothing_EqualZero()
        {
            // Arrange
            string uri = "LevelsEntities?$expand=Parent($levels=0)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken entities = result["value"];
            Assert.Equal(10, entities.Count());
            AssertEntity(entities[0], 1);
            Assert.Null(entities[0]["Parent"]);
            AssertDerivedEntity(entities[1], 2);
            Assert.Null(entities[1]["Parent"]);
        }

        [Fact]
        public async Task Levels_Throws_ExcceedsMaxExpandLevel()
        {
            // Arrange
            string uri = "LevelsEntities?$expand=Parent($levels=20)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string result = await response.Content.ReadAsStringAsync();
            Assert.Equal(
                "The query specified in the URI is not valid. The request includes a $expand path which is too deep. " +
                "The maximum depth allowed is 5. To increase the limit, set the 'MaxExpansionDepth' property on " +
                "EnableQueryAttribute or ODataValidationSettings, or set the 'MaxDepth' property in ExpandAttribute.",
                result);
        }

        [Fact]
        public async Task Levels_ExpandsAllLevels_DollarLevelEqualToActualLevel()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=5)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            // Level 1
            AssertEntity(result["Parent"], 5);
            // Level 2
            AssertDerivedEntity(result["Parent"]["Parent"], 4);
            // Level 3
            AssertEntity(result["Parent"]["Parent"]["Parent"], 3);
            // Level 4
            AssertDerivedEntity(result["Parent"]["Parent"]["Parent"]["Parent"], 2);
            // Level 5
            AssertEntity(result["Parent"]["Parent"]["Parent"]["Parent"]["Parent"], 1);
            // No further expanding.
            Assert.Null(result["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_ExpandsToNull_DollarLevelGreaterThanActualLevel()
        {
            // Arrange
            string uri = "LevelsEntities?$expand=Parent($levels=5)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken entities = result["value"];
            // Level 1
            AssertDerivedEntity(entities[2]["Parent"], 2);
            // Level 2
            AssertDerivedEntity(entities[2]["Parent"], 2);
            // Stop expanding for null.
            AssertNullValue(entities[2]["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_ExpandsToLevels_DollarLevelLessThanActualLevel()
        {
            // Arrange
            string uri = "LevelsEntities(5)?$expand=Parent($levels=2)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            // Level 1
            AssertDerivedEntity(result["Parent"], 4);
            // Level 2
            AssertEntity(result["Parent"]["Parent"], 3);
            // No further expanding.
            Assert.Null(result["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_Works_MaxDollarLevel()
        {
            // Arrange
            string uri = "LevelsEntities(5)?$expand=Parent($levels=max)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            // Level 1
            AssertDerivedEntity(result["Parent"], 4);
            // Level 2
            AssertEntity(result["Parent"]["Parent"], 3);
            // Level 3
            AssertDerivedEntity(result["Parent"]["Parent"]["Parent"], 2);
            // Level 4
            AssertEntity(result["Parent"]["Parent"]["Parent"]["Parent"], 1);
            // Stop expanding for null.
            AssertNullValue(result["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_ExpandsToLevels_LoopExists()
        {
            // Arrange
            string uri = "LevelsEntities(9)?$expand=Parent($expand=Parent($levels=2))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            // Level 1
            AssertDerivedEntity(result["Parent"], 10);
            // Level 2
            AssertEntity(result["Parent"]["Parent"], 9);
            // Level 3
            AssertDerivedEntity(result["Parent"]["Parent"]["Parent"], 10);
            // No further expanding.
            Assert.Null(result["Parent"]["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_ExpandsToMaxExpandLevel_LoopExists()
        {
            // Arrange
            string uri = "LevelsEntities(9)?$expand=Parent($levels=max)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            // Level 1
            AssertDerivedEntity(result["Parent"], 10);
            // Level 2
            AssertEntity(result["Parent"]["Parent"], 9);
            // Level 3
            AssertDerivedEntity(result["Parent"]["Parent"]["Parent"], 10);
            // Level 4
            AssertEntity(result["Parent"]["Parent"]["Parent"]["Parent"], 9);
            // Level 5
            AssertDerivedEntity(result["Parent"]["Parent"]["Parent"]["Parent"]["Parent"], 10);
            // No further expanding.
            Assert.Null(result["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_Works_ExpandsBaseTypeProperty()
        {
            // Arrange
            string uri = "LevelsEntities(2)?$expand=BaseEntities($levels=3)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken baseEntities = result["BaseEntities"];
            Assert.Equal(2, baseEntities.Count());
            // Level 1
            AssertEntity(baseEntities[0], 1);
            Assert.Single(baseEntities[0]["BaseEntities"]);
            // Level 2
            AssertEntity(baseEntities[0]["BaseEntities"][0], 11);
            // No further expanding
            Assert.Null(baseEntities[0]["BaseEntities"][0]["BaseEntities"]);
            // Level 1
            AssertEntity(baseEntities[1], 12);
            // No further expanding
            Assert.Null(baseEntities[1]["BaseEntities"]);
        }

        [Fact]
        public async Task Levels_Works_ExpandsDerivedTypeProperty()
        {
            // Arrange
            string uri = "LevelsEntities(5)?$expand=DerivedAncestors($levels=3)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken derivedEntities = result["DerivedAncestors"];
            Assert.Equal(2, derivedEntities.Count());
            // Level 1
            AssertDerivedEntity(derivedEntities[0], 2);
            // Level 2
            AssertDerivedEntity(derivedEntities[0]["DerivedAncestors"][0]["DerivedAncestors"][0], 2);
            // Level 1
            AssertDerivedEntity(derivedEntities[1], 4);
            // Level 2
            Assert.Single(derivedEntities[1]["DerivedAncestors"]);
            AssertDerivedEntity(derivedEntities[1]["DerivedAncestors"][0], 2);
            // Level 3
            AssertDerivedEntity(derivedEntities[1]["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
        }

        [Fact]
        public async Task Levels_Works_WithTypeCast()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Microsoft.AspNet.OData.Test.Routing.LevelsDerivedEntity/AncestorsInDerivedEntity($levels=2)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken derivedEntities = result["AncestorsInDerivedEntity"];
            Assert.Equal(5, derivedEntities.Count());
            // Level 1
            AssertEntity(derivedEntities[0], 1);
            // Stop expanding when casting fails.
            Assert.Null(derivedEntities[0]["AncestorsInDerivedEntity"]);
            // Level 1
            AssertDerivedEntity(derivedEntities[1], 2);
            // Level 2
            Assert.Single(derivedEntities[1]["AncestorsInDerivedEntity"]);
            AssertEntity(derivedEntities[1]["AncestorsInDerivedEntity"][0], 1);
            // No further expanding.
            Assert.Null(derivedEntities[1]["AncestorsInDerivedEntity"][0]["AncestorsInDerivedEntity"]);
            // Level 1
            AssertEntity(derivedEntities[2], 3);
            // Level 1
            AssertDerivedEntity(derivedEntities[3], 4);
            // Level 2
            Assert.Equal(3, derivedEntities[3]["AncestorsInDerivedEntity"].Count());
            AssertEntity(derivedEntities[3]["AncestorsInDerivedEntity"][0], 1);
            // Stop expanding when casting fails.
            Assert.Null(derivedEntities[3]["AncestorsInDerivedEntity"][0]["AncestorsInDerivedEntity"]);
            AssertDerivedEntity(derivedEntities[3]["AncestorsInDerivedEntity"][1], 2);
            // No further expanding.
            Assert.Null(derivedEntities[3]["AncestorsInDerivedEntity"][1]["AncestorsInDerivedEntity"]);
            AssertEntity(derivedEntities[3]["AncestorsInDerivedEntity"][2], 3);
            // Stop expanding when casting fails.
            Assert.Null(derivedEntities[3]["AncestorsInDerivedEntity"][2]["AncestorsInDerivedEntity"]);
            // Level 1
            AssertEntity(derivedEntities[4], 5);
            // Stop expanding when casting fails.
            Assert.Null(derivedEntities[4]["AncestorsInDerivedEntity"]);
        }

        [Fact]
        public async Task Levels_AppliesSameSelectForEachLevel()
        {
            // Arrange
            string uri = "LevelsEntities(5)?$select=ID&$expand=Parent($levels=2;$select=Name)";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(5, result["ID"]);
            Assert.Null(result["Name"]);
            // Level 1
            Assert.Null(result["Parent"]["ID"]);
            Assert.Equal("Name 4", result["Parent"]["Name"]);
            Assert.Null(result["Parent"]["DerivedName"]);
            // Level 2
            Assert.Null(result["Parent"]["Parent"]["ID"]);
            Assert.Equal("Name 3", result["Parent"]["Parent"]["Name"]);
            // No further expanding.
            Assert.Null(result["Parent"]["Parent"]["Parent"]);
        }

        [Fact]
        public async Task Levels_Works_WithNestedLevels()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=2;$expand=DerivedAncestors($levels=2))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];
            // Level 1
            AssertEntity(parent, 5);
            // Level 2
            AssertDerivedEntity(parent["Parent"], 4);
            // No further expanding.
            Assert.Null(parent["Parent"]["Parent"]);
            // Level 1
            Assert.Equal(2, parent["DerivedAncestors"].Count());
            AssertDerivedEntity(parent["DerivedAncestors"][0], 2);
            // Level 2
            AssertDerivedEntity(parent["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
            // Level 1
            AssertDerivedEntity(parent["DerivedAncestors"][1], 4);
            // Level 2
            AssertDerivedEntity(parent["DerivedAncestors"][1]["DerivedAncestors"][0], 2);
            // No further expanding.
            Assert.Null(parent["DerivedAncestors"][1]["DerivedAncestors"][0]["DerivedAncestors"]);
            // Level 1
            Assert.Single(parent["Parent"]["DerivedAncestors"]);
            AssertDerivedEntity(parent["Parent"]["DerivedAncestors"][0], 2);
            // Level 2
            Assert.Single(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMaxLevelInNestedExpand()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=3;$expand=DerivedAncestors($levels=max))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];
            
            // Level 3
            AssertEntity(parent["Parent"]["Parent"], 3);
            // No furthur expanding for "Parent"
            Assert.Null(parent["Parent"]["Parent"]["Parent"]);
            // Level 5
            AssertDerivedEntity(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
            // Level 5
            AssertDerivedEntity(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0], 2);
            // Level 5
            AssertDerivedEntity(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
            // No further expanding.
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMaxLevelInEveryExpand()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=max;$expand=DerivedAncestors($levels=max))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            // Level 5
            AssertEntity(parent["Parent"]["Parent"]["Parent"]["Parent"], 1);
            // No further expanding on level5 Parent
            Assert.Null(parent["Parent"]["Parent"]["Parent"]["Parent"]["Parent"]);
            Assert.Null(parent["Parent"]["Parent"]["Parent"]["Parent"]["DerivedAncestors"]);
            // Level 5
            AssertDerivedEntity(parent["Parent"]["Parent"]["Parent"]["DerivedAncestors"][0], 4);
            // Level 5
            AssertDerivedEntity(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
            // Level 5
            AssertDerivedEntity(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0], 2);
            // Level 5
            AssertDerivedEntity(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0], 4);
            // No further expanding.
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_SelectWithNestedMaxLevels()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($select=ID;$levels=3;$expand=DerivedAncestors($levels=max;$select=DerivedName))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            // Level 3
            Assert.Equal(3, parent["Parent"]["Parent"]["ID"]);
            Assert.Null(parent["Parent"]["Parent"]["Name"]);
            // No furthur expanding for "Parent"
            Assert.Null(parent["Parent"]["Parent"]["Parent"]);
            // Level 5
            Assert.Equal("DerivedName 4", parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedName"]);
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["ID"]);
            // Level 5
            Assert.Equal("DerivedName 2", parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedName"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["ID"]);
            // Level 5
            Assert.Equal("DerivedName 4", parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedName"]);
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["Name"]);
            // No further expanding.
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMaxOptionInOuterExpand()
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=max;$expand=DerivedAncestors($levels=2))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMultiParallelExpand() 
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=max;$expand=DerivedAncestors($levels=2),BaseEntities($levels=3))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            // "Parent" => 2, "BaseEntities" => 3, "DerivedAncestors" => 2
            Assert.Null(parent["Parent"]["BaseEntities"][1]["BaseEntities"][0]["BaseEntities"][0]["BaseEntities"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);

            // "Parent" => 1, "BaseEntities" => 3, "DerivedAncestors" => 2
            Assert.Null(parent["BaseEntities"][2]["BaseEntities"][1]["BaseEntities"][0]["BaseEntities"]);
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMaxLevelInMultiParallelExpand() 
        {
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=max;$expand=DerivedAncestors($levels=2),BaseEntities($levels=max))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            // "Parent" => 3, "BaseEntities" => 2, "DerivedAncestors" => 2
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
            Assert.Null(parent["Parent"]["Parent"]["BaseEntities"][0]["BaseEntities"][0]["BaseEntities"]);

            // "Parent" => 2, "BaseEntities" => 3, "DerivedAncestors" => 2
            Assert.Null(parent["Parent"]["BaseEntities"][1]["BaseEntities"][0]["BaseEntities"][0]["BaseEntities"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);

            // "Parent" => 1, "BaseEntities" => 4, "DerivedAncestors" => 2
            Assert.Null(parent["BaseEntities"][2]["BaseEntities"][1]["BaseEntities"][0]["BaseEntities"][0]["BaseEntities"]);
            Assert.Null(parent["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
        }

        [Fact]
        public async Task Levels_Works_WithMultiLevelsExpand()
        { 
            // Arrange
            string uri = "LevelsEntities(6)?$expand=Parent($levels=max;$expand=DerivedAncestors($levels=2;$expand=BaseEntities($levels=max)))";

            // Act
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/" + uri);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            JObject result = await response.Content.ReadAsObject<JObject>();
            JToken parent = result["Parent"];

            // "Parent" => 3, "DerivedAncestors" => 2, max("BaseEntities") => 1
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["BaseEntities"]);
            Assert.NotNull(parent["Parent"]["Parent"]["DerivedAncestors"][0]["BaseEntities"]);
            Assert.Null(parent["Parent"]["Parent"]["DerivedAncestors"][0]["BaseEntities"][0]["BaseEntities"]);

            // "Parent" => 2, "DerivedAncestors" => 2, max("BaseEntities") => 2
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["DerivedAncestors"]);
            Assert.NotNull(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["BaseEntities"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["DerivedAncestors"][0]["BaseEntities"][0]["BaseEntities"]);
            Assert.NotNull(parent["Parent"]["DerivedAncestors"][0]["BaseEntities"]);
            Assert.NotNull(parent["Parent"]["DerivedAncestors"][0]["BaseEntities"][0]["BaseEntities"]);
            Assert.Null(parent["Parent"]["DerivedAncestors"][0]["BaseEntities"][0]["BaseEntities"][0]["BaseEntities"]);

            // "Parent" => 1, "DerivedAncestors" => 2, max("BaseEntities") => 3
            Assert.Null(parent["DerivedAncestors"][1]["BaseEntities"][2]["BaseEntities"][1]["BaseEntities"][0]["BaseEntities"]);
        }

        private void AssertEntity(JToken entity, int key)
        {
            Assert.Equal(key, entity["ID"]);
            Assert.Equal("Name " + key, entity["Name"]);
        }

        private void AssertDerivedEntity(JToken entity, int key)
        {
            AssertEntity(entity, key);
            Assert.Equal("DerivedName " + key, entity["DerivedName"]);
        }

        private void AssertNullValue(JToken token)
        {
            JValue value = Assert.IsType<JValue>(token);
            Assert.Null(value.Value);
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<LevelsBaseEntity>("LevelsBaseEntities");
            builder.EntitySet<LevelsEntity>("LevelsEntities");
            builder.EntitySet<LevelsDerivedEntity>("LevelsDerivedEntities");
            return builder.GetEdmModel();
        }

        public class LevelsEntitiesController : TestODataController
        {
            public IList<LevelsEntity> Entities;

            public LevelsEntitiesController()
            {
                Entities = new List<LevelsEntity>();
                for (int i = 1; i <= 10; i++)
                {
                    if (i % 2 == 1)
                    {
                        var newEntity = new LevelsEntity
                        {
                            ID = i,
                            Name = "Name " + i,
                            Parent = Entities.LastOrDefault(),
                            BaseEntities = Entities.Concat(new[]
                                {
                                    new LevelsBaseEntity
                                    {
                                        ID = i + 10,
                                        Name = "Name " + (i + 10)
                                    }
                                }).ToArray(),
                            DerivedAncestors = Entities.OfType<LevelsDerivedEntity>().ToArray()
                        };
                        Entities.Add(newEntity);
                    }
                    else
                    {
                        var newEntity = new LevelsDerivedEntity
                        {
                            ID = i,
                            Name = "Name " + i,
                            DerivedName = "DerivedName " + i,
                            Parent = Entities.LastOrDefault(),
                            BaseEntities = Entities.Concat(new[]
                                {
                                    new LevelsBaseEntity
                                    {
                                        ID = i + 10,
                                        Name = "Name " + (i + 10)
                                    }
                                }).ToArray(),
                            DerivedAncestors = Entities.OfType<LevelsDerivedEntity>().ToArray(),
                            AncestorsInDerivedEntity = Entities.ToArray()
                        };
                        Entities.Add(newEntity);
                    }
                }
                Entities[8].Parent = Entities[9];
                Entities[1].DerivedAncestors = new LevelsDerivedEntity[] { (LevelsDerivedEntity)Entities[3] };
            }

#if !NETCORE // TODO #939: Fix Get method to return non-null.
            public IHttpActionResult Get(ODataQueryOptions<LevelsEntity> queryOptions)
            {
                var validationSettings = new ODataValidationSettings { MaxExpansionDepth = 5 };

                try
                {
                    queryOptions.Validate(validationSettings);
                }
                catch (ODataException e)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.Content = new StringContent(
                        Error.Format("The query specified in the URI is not valid. {0}", e.Message));
                    return ResponseMessage(responseMessage);
                }

                var querySettings = new ODataQuerySettings();
                var result = queryOptions.ApplyTo(Entities.AsQueryable(), querySettings).AsQueryable();
                return Ok(result, result.GetType());
            }
#else
            public ITestActionResult Get(ODataQueryOptions<LevelsEntity> queryOptions)
            {
                var validationSettings = new ODataValidationSettings { MaxExpansionDepth = 5 };

                try
                {
                    queryOptions.Validate(validationSettings);
                }
                catch (ODataException e)
                {
                    string error = Error.Format("The query specified in the URI is not valid. {0}", e.Message);
                    return BadRequest(error);
                }

                var querySettings = new ODataQuerySettings();
                var result = queryOptions.ApplyTo(Entities.AsQueryable(), querySettings).AsQueryable();
                return Ok(result, result.GetType());
            }
#endif

            [EnableQuery(MaxExpansionDepth = 5)]
            public ITestActionResult Get(int key)
            {
                return Ok(Entities.Single(e => e.ID == key));
            }

#if !NETCORE // TODO #939: Fix Ok method to return non-null.
            private IHttpActionResult Ok(object content, Type type)
            {
                var resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(type);
                return Activator.CreateInstance(resultType, content, this) as IHttpActionResult;
            }
#else
            private ITestActionResult Ok(object content, Type type)
            {
                return Ok(content);
            }
#endif
        }

        public class LevelsBaseEntity
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class LevelsEntity : LevelsBaseEntity
        {
            public LevelsEntity Parent { get; set; }
            public LevelsBaseEntity[] BaseEntities { get; set; }
            public LevelsDerivedEntity[] DerivedAncestors { get; set; }
        }

        public class LevelsDerivedEntity : LevelsEntity
        {
            public string DerivedName { get; set; }
            public LevelsEntity[] AncestorsInDerivedEntity { get; set; }
        }
    }
}
