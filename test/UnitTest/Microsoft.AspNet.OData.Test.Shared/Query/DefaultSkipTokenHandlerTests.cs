//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.OData.Test.Query
{
    [DataContract]
    public enum SkipTokenTestAliasedGender
    {
        [EnumMember(Value = "M")]
        Male,
        [EnumMember(Value = "F")]
        Female
    }

    // Wrapper class exists purely so SkipTokenTestGender is a nested enum: its CLR
    // FullName contains a '+' (nested-type) separator, which reproduces the bug where
    // the generated skiptoken value used the CLR FullName instead of the EDM type name.
    public class SkipTokenTestGenderContainer
    {
        public enum SkipTokenTestGender
        {
            Male,
            Female
        }
    }

    public class SkipTokenEnumCustomer
    {
        public int Id { get; set; }
        public SkipTokenTestGenderContainer.SkipTokenTestGender Gender { get; set; }
        public SkipTokenTestAliasedGender AliasedGender { get; set; }
    }

    public class SkipTokenBoundaryCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PublicScore { get; set; }
        public decimal? CreditLimit { get; set; }
        public SkipTokenTestGenderContainer.SkipTokenTestGender Gender { get; set; }
        public int InternalRiskScore { get; set; }
        public string InternalTier { get; set; }
        public DateTime InternalDate { get; set; }

        [IgnoreDataMember]
        public int DmFlag { get; set; }

        [NotMapped]
        public int NotMappedFlag { get; set; }

        public int RoFlag { get { return 77; } }
        public int PrivSet { get; private set; }

        public SkipTokenBoundaryCustomer()
        {
            PrivSet = 44;
        }

        internal static IQueryable<SkipTokenBoundaryCustomer> GetCustomers()
        {
            return Enumerable.Range(1, 4).Select(id => new SkipTokenBoundaryCustomer
            {
                Id = id,
                Name = "customer-" + id,
                PublicScore = id * 10,
                CreditLimit = id <= 2 ? (decimal?)null : id * 10,
                Gender = id % 2 == 0 ? SkipTokenTestGenderContainer.SkipTokenTestGender.Female : SkipTokenTestGenderContainer.SkipTokenTestGender.Male,
                InternalRiskScore = id * 20,
                InternalTier = "hidden-tier-" + id,
                InternalDate = new DateTime(2020, 1, id, 0, 0, 0, DateTimeKind.Utc),
                DmFlag = id,
                NotMappedFlag = id
            }).AsQueryable();
        }
    }

    public class SkipTokenBoundaryCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 2)]
        public IQueryable<SkipTokenBoundaryCustomer> Get()
        {
            return SkipTokenBoundaryCustomer.GetCustomers();
        }
    }

    public class SkipTokenCompositeCustomer
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
    }

    public class DefaultSkipTokenHandlerBoundaryTests
    {
        private readonly HttpClient _client;

        public DefaultSkipTokenHandlerBoundaryTests()
        {
            var server = TestServerFactory.Create(new[] { typeof(SkipTokenBoundaryCustomersController) }, config =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(config);
                var type = builder.EntitySet<SkipTokenBoundaryCustomer>("SkipTokenBoundaryCustomers").EntityType;
                type.Ignore(row => row.InternalRiskScore);
                type.Ignore(row => row.InternalTier);
                type.Ignore(row => row.InternalDate);
                type.Ignore(row => row.RoFlag);
                type.Ignore(row => row.PrivSet);
                type.Property(row => row.Name).Name = "DisplayName";
                config.SkipToken().OrderBy().Select().MaxTop(null);
                config.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            });
            _client = TestServerFactory.CreateClient(server);
        }

        [Theory]
        [InlineData("InternalRiskScore-50")]
        [InlineData("InternalRiskScore-90")]
        [InlineData("InternalTier-'p'")]
        [InlineData("InternalDate-2020-01-01T00:00:00Z")]
        [InlineData("DmFlag-2")]
        [InlineData("NotMappedFlag-2")]
        [InlineData("RoFlag-50")]
        [InlineData("PrivSet-20")]
        [InlineData("NoSuchProp-1")]
        [InlineData("Id-2,InternalRiskScore-44")]
        [InlineData("Id-2,InternalRiskScore-45")]
        [InlineData("PublicScore-20")]
        [InlineData("DisplayName-'customer-2'")]
        [InlineData("Id-1,Id-2")]
        [InlineData("Id-1,NoSuchProp-2")]
        [InlineData("Id-'wrong-type'")]
        [InlineData("Id-null")]
        [InlineData("Id-2147483648")]
        [InlineData("Id")]
        [InlineData("-1")]
        public async Task ApplyTo_RejectsForgedSkipTokens(string token)
        {
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/SkipTokenBoundaryCustomers?$skiptoken=" + Uri.EscapeDataString(token));

            string content = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest, content);
            Assert.NotNull(JObject.Parse(content)["error"]);
            Assert.DoesNotContain("hidden-tier", content);
        }

        [Theory]
        [InlineData("Id-2,DisplayName-'customer-2'")]
        [InlineData("DisplayName-'customer-2'")]
        [InlineData("InternalTier-'p',Id-2")]
        [InlineData("DisplayName-'customer-2',Id-2,InternalRiskScore-44")]
        public async Task ApplyTo_RejectsTokensOutsideStableOrder(string token)
        {
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/SkipTokenBoundaryCustomers?$orderby=DisplayName&$skiptoken=" + Uri.EscapeDataString(token));

            string content = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest, content);
            Assert.NotNull(JObject.Parse(content)["error"]);
        }

        [Theory]
        [InlineData("", 1, 2, 3, 4)]
        [InlineData("?$orderby=DisplayName", 1, 2, 3, 4)]
        [InlineData("?$orderby=DisplayName desc", 4, 3, 2, 1)]
        [InlineData("?$orderby=PublicScore desc", 4, 3, 2, 1)]
        [InlineData("?$orderby=CreditLimit", 1, 2, 3, 4)]
        [InlineData("?$orderby=CreditLimit desc", 4, 3, 1, 2)]
        [InlineData("?$orderby=Gender", 1, 3, 2, 4)]
        [InlineData("?$select=Id,DisplayName", 1, 2, 3, 4)]
        public async Task ApplyTo_FollowsServerGeneratedNextLinks(string query, int first, int second, int third, int fourth)
        {
            string uri = "http://localhost/odata/SkipTokenBoundaryCustomers" + query;
            var ids = new List<int>();
            for (int page = 0; page < 2; page++)
            {
                HttpResponseMessage response = await _client.GetAsync(uri);
                string content = await response.Content.ReadAsStringAsync();
                Assert.True(response.StatusCode == HttpStatusCode.OK, content);
                JObject result = JObject.Parse(content);
                JArray values = (JArray)result["value"];
                Assert.Equal(2, values.Count);
                ids.AddRange(values.Select(row => row["Id"].Value<int>()));
                Assert.DoesNotContain("InternalRiskScore", content);
                Assert.DoesNotContain("InternalTier", content);
                Assert.DoesNotContain("DmFlag", content);
                if (page == 0)
                {
                    uri = result["@odata.nextLink"].Value<string>();
                    Assert.Contains("$skiptoken=", uri);
                }
                else
                {
                    Assert.Null(result["@odata.nextLink"]);
                }
            }

            Assert.Equal(new[] { first, second, third, fourth }, ids);
        }

        [Fact]
        public async Task ApplyTo_AcceptsModeledKeyToken()
        {
            HttpResponseMessage response = await _client.GetAsync("http://localhost/odata/SkipTokenBoundaryCustomers?$skiptoken=Id-2");
            string content = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, content);
            Assert.Equal(new[] { 3, 4 }, JObject.Parse(content)["value"].Select(row => row["Id"].Value<int>()));
        }

        [Fact]
        public void ApplyTo_UsesRenamedCompositeKeysWithoutQueryOptions()
        {
            var builder = new ODataConventionModelBuilder();
            var entity = builder.EntitySet<SkipTokenCompositeCustomer>("Customers").EntityType;
            entity.HasKey(row => new { row.Id, row.OtherId });
            entity.Property(row => row.Id).Name = "Key";
            entity.Property(row => row.OtherId).Name = "OtherKey";
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(SkipTokenCompositeCustomer));
            var parser = new ODataQueryOptionParser(model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$skiptoken", "Key-1,OtherKey-2" } });
            var option = new SkipTokenQueryOption("Key-1,OtherKey-2", context, parser);
            IQueryable query = new[]
            {
                new SkipTokenCompositeCustomer { Id = 1, OtherId = 1 },
                new SkipTokenCompositeCustomer { Id = 1, OtherId = 2 },
                new SkipTokenCompositeCustomer { Id = 2, OtherId = 1 }
            }.AsQueryable();

            IQueryable result = option.ApplyTo(query, new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }, null);

            Assert.Equal(2, Assert.Single(result.Cast<SkipTokenCompositeCustomer>()).Id);
        }

        [Theory]
        [InlineData("InternalRiskScore-50")]
        [InlineData("NoSuchProp-1")]
        [InlineData("Id-2,InternalRiskScore-44")]
        public void ApplyTo_RejectsForgedNamesWithoutQueryOptions(string token)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenBoundaryCustomer>("Customers").EntityType.Ignore(row => row.InternalRiskScore);
            IEdmModel model = builder.GetEdmModel();
            var context = new ODataQueryContext(model, typeof(SkipTokenBoundaryCustomer));
            var parser = new ODataQueryOptionParser(model, context.ElementType, context.NavigationSource,
                new Dictionary<string, string> { { "$skiptoken", token } });
            var option = new SkipTokenQueryOption(token, context, parser);
            IQueryable query = SkipTokenBoundaryCustomer.GetCustomers();

            Assert.Throws<ODataException>(() => option.ApplyTo(query,
                new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False }, null));
        }
    }

    public class DefaultSkipTokenHandlerTests
    {
        [Theory]
        [InlineData("http://localhost/Customers(1)/Orders", "http://localhost/Customers(1)/Orders?$skip=10")]
        [InlineData("http://localhost/Customers?$expand=Orders", "http://localhost/Customers?$expand=Orders&$skip=10")]
        public void GetNextPageLink_ReturnsCorrectNextLink(string baseUri, string expectedUri)
        {
            // Arrange
            var context = GetContext(false);
            var nextLinkGenerator = context.QueryContext.GetSkipTokenHandler();

            // Act
            var uri = nextLinkGenerator.GenerateNextPageLink(new Uri(baseUri), 10, null, context);
            var actualUri = uri.ToString();

            // Assert
            Assert.Equal(expectedUri, actualUri);
        }

        private ODataSerializerContext GetContext(bool enableSkipToken = false)
        {
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmEntitySet entitySet = model.Customers;
            IEdmEntityType entityType = entitySet.EntityType();
            IEdmProperty edmProperty = entityType.FindProperty("Name");
            IEdmType edmType = entitySet.Type;
            ODataPath path = new ODataPath(new EntitySetSegment(entitySet));
            ODataQueryContext queryContext = new ODataQueryContext(model.Model, edmType, path);
            queryContext.DefaultQuerySettings.EnableSkipToken = enableSkipToken;

            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            ResourceContext resource = new ResourceContext();
            ODataSerializerContext context = new ODataSerializerContext(resource, edmProperty, queryContext, null);
            return context;
        }

        [Fact]
        public void GenerateSkipTokenValue_UsesEdmEnumTypeName_NotClrFullName_ForEnumProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenEnumCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "SkipTokenEnumCustomer");
            IEdmProperty genderProperty = customerType.FindProperty("Gender");

            SkipTokenEnumCustomer instance = new SkipTokenEnumCustomer { Id = 1, Gender = SkipTokenTestGenderContainer.SkipTokenTestGender.Male };
            OrderByPropertyNode orderByNode = new OrderByPropertyNode(genderProperty, OrderByDirection.Ascending);

            // Act
            string skipTokenValue = InvokeGenerateSkipTokenValue(instance, model, new[] { orderByNode });

            // Assert: EDM type name is used (dot-separated), not the CLR FullName ('+'-separated).
            Assert.Contains("Gender-", skipTokenValue);
            Assert.Contains(".SkipTokenTestGender%27Male%27", skipTokenValue);
            Assert.DoesNotContain("%2B", skipTokenValue, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GenerateSkipTokenValue_UsesEdmMemberAlias_ForEnumMemberAnnotatedProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SkipTokenEnumCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "SkipTokenEnumCustomer");
            IEdmProperty aliasedGenderProperty = customerType.FindProperty("AliasedGender");

            SkipTokenEnumCustomer instance = new SkipTokenEnumCustomer { Id = 1, AliasedGender = SkipTokenTestAliasedGender.Male };
            OrderByPropertyNode orderByNode = new OrderByPropertyNode(aliasedGenderProperty, OrderByDirection.Ascending);

            // Act
            string skipTokenValue = InvokeGenerateSkipTokenValue(instance, model, new[] { orderByNode });

            // Assert: the EDM alias 'M' (from [EnumMember(Value = "M")]) is used, not the CLR name 'Male'.
            Assert.Contains("AliasedGender-", skipTokenValue);
            Assert.Contains("%27M%27", skipTokenValue);
            Assert.DoesNotContain("%27Male%27", skipTokenValue);
        }

        private static string InvokeGenerateSkipTokenValue(object lastMember, IEdmModel model, OrderByNode[] orderByNodes)
        {
            MethodInfo method = typeof(DefaultSkipTokenHandler).GetMethod(
                "GenerateSkipTokenValue", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method.Invoke(null, new object[] { lastMember, model, orderByNodes });
        }
    }
}
