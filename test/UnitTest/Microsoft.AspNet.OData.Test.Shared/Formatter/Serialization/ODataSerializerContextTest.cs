// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataSerializerContextTest
    {
        [Fact]
        public void EmptyCtor_DoesnotThrow()
        {
            ExceptionAssert.DoesNotThrow(() => new ODataSerializerContext());
        }

        [Fact]
        public void Ctor_ForNestedContext_ThrowsArgumentNull_Resource()
        {
            // Arrange
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = new Mock<IEdmNavigationProperty>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataSerializerContext(resource: null, selectExpandClause: selectExpand, edmProperty: navProp), "resource");
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_CopiesProperties()
        {
            // Arrange
            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = model.Customers,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Model = model.Model,
                Path = new ODataPath(),
                Request = request,
                RootElementName = "somename",
                SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true),
                SkipExpensiveAvailabilityChecks = true,
#if NETFX // Url is only in AspNet
                Url = new UrlHelper()
#endif
            };
            ResourceContext resource = new ResourceContext { SerializerContext = context };
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = model.Customer.NavigationProperties().First();

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, navProp);

            // Assert
            Assert.Equal(context.MetadataLevel, nestedContext.MetadataLevel);
            Assert.Same(context.Model, nestedContext.Model);
            Assert.Same(context.Path, nestedContext.Path);
            Assert.Same(context.Request, nestedContext.Request);
            Assert.Equal(context.RootElementName, nestedContext.RootElementName);
            Assert.Equal(context.SkipExpensiveAvailabilityChecks, nestedContext.SkipExpensiveAvailabilityChecks);
#if NETFX // Url is only in AspNet
            Assert.Same(context.Url, nestedContext.Url);
#endif
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_InitializesRightValues()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = model.Customer.NavigationProperties().First();
            ODataSerializerContext context = new ODataSerializerContext { NavigationSource = model.Customers, Model = model.Model };
            ResourceContext resource = new ResourceContext { SerializerContext = context };

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, navProp);

            // Assert
            Assert.Same(resource, nestedContext.ExpandedResource);
            Assert.Same(navProp, nestedContext.NavigationProperty);
            Assert.Same(selectExpand, nestedContext.SelectExpandClause);
            Assert.Same(model.Orders, nestedContext.NavigationSource);
            Assert.Same(navProp, nestedContext.EdmProperty);
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_InitializesRightValues_ForComplex()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmProperty complexProperty = model.Customer.Properties().First(p => p.Name == "Address");
            ODataSerializerContext context = new ODataSerializerContext { NavigationSource = model.Customers, Model = model.Model };
            ResourceContext resource = new ResourceContext { SerializerContext = context };

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, complexProperty);

            // Assert
            Assert.Same(resource, nestedContext.ExpandedResource);
            Assert.Same(selectExpand, nestedContext.SelectExpandClause);
            Assert.Same(complexProperty, nestedContext.EdmProperty);
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
            NullEdmType edmObject = new NullEdmType();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => context.GetEdmType(edmObject, null),
                exceptionMessage: "The EDM type of the object of type 'Microsoft.AspNet.OData.Test.Formatter.Serialization.ODataSerializerContextTest+NullEdmType'" +
                " is null. The EDM type of an IEdmObject cannot be null.");
        }

        /// <summary>
        /// An instance of IEdmObject with no EdmType.
        /// </summary>
        private class NullEdmType : IEdmObject
        {
            public IEdmTypeReference GetEdmType()
            {
                return null;
            }
        }
    }
}
