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
