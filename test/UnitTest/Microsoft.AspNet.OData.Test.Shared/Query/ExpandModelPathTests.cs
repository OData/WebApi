//-----------------------------------------------------------------------------
// <copyright file="ExpandModelPathTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ExpandModelPathTests
    {
        private IEdmEntityType _customerType;
        private IEdmComplexType _addressType;
        private IEdmNavigationProperty _relatedNavProperty;
        private IEdmStructuralProperty _homeAddress;

        public ExpandModelPathTests()
        {
            // Address is an open complex type
            var addressType = new EdmComplexType("NS", "Address");
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            _addressType = addressType;

            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            _homeAddress = customerType.AddStructuralProperty("HomeAddress", new EdmComplexTypeReference(addressType, false));
            _customerType = customerType;

            _relatedNavProperty = addressType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "RelatedCustomers",
                Target = customerType,
                TargetMultiplicity = EdmMultiplicity.Many
            });
        }

        [Fact]
        public void Ctor_ExpandModelPath_WithBasicElements_SetsProperties()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _relatedNavProperty
            };

            // Act
            ExpandModelPath expandPath = new ExpandModelPath(elements);

            // Assert
            Assert.Same(_relatedNavProperty, expandPath.Navigation);
            Assert.Equal("RelatedCustomers", expandPath.ExpandPath);
            Assert.Equal("", expandPath.NavigationPropertyPath);
        }

        [Fact]
        public void Ctor_ExpandModelPath_WithOtherElements_SetsProperties()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _homeAddress,
                _relatedNavProperty
            };

            // Act
            ExpandModelPath expandPath = new ExpandModelPath(elements);

            // Assert
            Assert.Same(_relatedNavProperty, expandPath.Navigation);
            Assert.Equal("HomeAddress/RelatedCustomers", expandPath.ExpandPath);
            Assert.Equal("HomeAddress", expandPath.NavigationPropertyPath);
        }

        [Fact]
        public void Ctor_ExpandModelPath_WithTypeElements_SetsProperties()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _homeAddress,
                _customerType, // it's not valid from the model, but it's valid for the test.
                _relatedNavProperty
            };

            // Act
            ExpandModelPath expandPath = new ExpandModelPath(elements);

            // Assert
            Assert.Same(_relatedNavProperty, expandPath.Navigation);
            Assert.Equal("HomeAddress/NS.Customer/RelatedCustomers", expandPath.ExpandPath);
            Assert.Equal("HomeAddress/NS.Customer", expandPath.NavigationPropertyPath);
        }

        [Fact]
        public void Ctor_ExpandModelPath_Throws_EmtpyElement()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>();

            // Act
            Action test = () => new ExpandModelPath(elements);

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                "A navigation property expand path should have navigation property in the path.");
        }

        [Fact]
        public void Ctor_ExpandModelPath_Throws_WithoutNavigationElementOnLast()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _homeAddress
            };

            // Act
            Action test = () => new ExpandModelPath(elements);

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                "The last segment 'EdmStructuralProperty' of the select or expand query option is not supported.");
        }

        [Fact]
        public void Ctor_ExpandModelPath_Throws_OnlyWithTypeCastElementOnLast()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _addressType
            };

            // Act
            Action test = () => new ExpandModelPath(elements);

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                "The last segment 'EdmComplexType' of the select or expand query option is not supported.");
        }
    }
}
