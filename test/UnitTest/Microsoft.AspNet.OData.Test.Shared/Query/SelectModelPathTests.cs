//-----------------------------------------------------------------------------
// <copyright file="SelectModelPathTests.cs" company=".NET Foundation">
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
    public class SelectModelPathTests
    {
        private IEdmStructuralProperty _homeAddress;
        private IEdmStructuralProperty _city;
        private IEdmNavigationProperty _relatedNavProperty;

        public SelectModelPathTests()
        {
            // Address is an open complex type
            var addressType = new EdmComplexType("NS", "Address");
            addressType.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            _city = addressType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);

            EdmEntityType customerType = new EdmEntityType("NS", "Customer");
            customerType.AddKeys(customerType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            _homeAddress = customerType.AddStructuralProperty("HomeAddress", new EdmComplexTypeReference(addressType, false));

            _relatedNavProperty = addressType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "RelatedCustomers",
                Target = customerType,
                TargetMultiplicity = EdmMultiplicity.Many
            });
        }

        [Fact]
        public void Ctor_SelectModelPath_WithBasicElements_SetsProperties()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _homeAddress
            };

            // Act
            SelectModelPath selectPath = new SelectModelPath(elements);

            // Assert
            Assert.Equal("HomeAddress", selectPath.SelectPath);
        }

        [Fact]
        public void Ctor_SelectModelPath_WithOtherElements_SetsProperties()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _homeAddress,
                _city
            };

            // Act
            SelectModelPath selectPath = new SelectModelPath(elements);

            // Assert
            Assert.Equal("HomeAddress/City", selectPath.SelectPath);
        }

        [Fact]
        public void Ctor_SelectModelPath_Throws_EmtpyElement()
        {
            // Arrange
            IList<IEdmElement> elements = new List<IEdmElement>
            {
                _relatedNavProperty
            };

            // Act
            Action test = () => new SelectModelPath(elements);

            // Assert
            ExceptionAssert.Throws<ODataException>(test,
                "A segment 'EdmNavigationProperty' within the select or expand query option is not supported.");
        }
    }
}
