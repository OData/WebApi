// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataSerializerContextTest
    {
        [Fact]
        public void EmptyCtor_DoesnotThrow()
        {
            Assert.DoesNotThrow(() => new ODataSerializerContext());
        }

        [Fact]
        public void Ctor_ForNestedContext_ThrowsArgumentNull_Entity()
        {
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = new Mock<IEdmNavigationProperty>().Object;

            Assert.ThrowsArgumentNull(
                () => new ODataSerializerContext(entity: null, selectExpandClause: selectExpand, navigationProperty: navProp), "entity");
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_CopiesProperties()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = model.Customers,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Model = model.Model,
                Path = new ODataPath(),
                Request = new HttpRequestMessage(),
                RootElementName = "somename",
                SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true),
                SkipExpensiveAvailabilityChecks = true,
                Url = new UrlHelper()
            };
            EntityInstanceContext entity = new EntityInstanceContext { SerializerContext = context };
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = model.Customer.NavigationProperties().First();

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpand, navProp);

            // Assert
            Assert.Equal(context.MetadataLevel, nestedContext.MetadataLevel);
            Assert.Same(context.Model, nestedContext.Model);
            Assert.Same(context.Path, nestedContext.Path);
            Assert.Same(context.Request, nestedContext.Request);
            Assert.Equal(context.RootElementName, nestedContext.RootElementName);
            Assert.Equal(context.SkipExpensiveAvailabilityChecks, nestedContext.SkipExpensiveAvailabilityChecks);
            Assert.Same(context.Url, nestedContext.Url);
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_InitializesRightValues()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = model.Customer.NavigationProperties().First();
            ODataSerializerContext context = new ODataSerializerContext { NavigationSource = model.Customers, Model = model.Model };
            EntityInstanceContext entity = new EntityInstanceContext { SerializerContext = context };

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpand, navProp);

            // Assert
            Assert.Same(entity, nestedContext.ExpandedEntity);
            Assert.Same(navProp, nestedContext.NavigationProperty);
            Assert.Same(selectExpand, nestedContext.SelectExpandClause);
            Assert.Same(model.Orders, nestedContext.NavigationSource);
        }

        [Fact]
        public void Property_Items_IsInitialized()
        {
            ODataSerializerContext context = new ODataSerializerContext();
            Assert.NotNull(context.Items);
        }

        [Fact]
        public void GetEdmType_ThrowsInvalidOperation_IfEdmObjectGetEdmTypeReturnsNull()
        {
            // Arrange (this code path does not use ODataSerializerContext fields or properties)
            var context = new ODataSerializerContext();
            Mock<IEdmObject> mock = new Mock<IEdmObject>(MockBehavior.Strict);
            mock.Setup(edmObject => edmObject.GetEdmType()).Returns<IEdmTypeReference>(null).Verifiable();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => context.GetEdmType(mock.Object, null),
                exceptionMessage: "The EDM type of the object of type 'Castle.Proxies.IEdmObjectProxy' is null. " +
                "The EDM type of an IEdmObject cannot be null.");
            mock.Verify();
        }
    }
}
