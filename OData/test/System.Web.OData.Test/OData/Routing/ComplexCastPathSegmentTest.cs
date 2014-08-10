// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class ComplexCastPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_CastType()
        {
            Assert.ThrowsArgumentNull(() => new ComplexCastPathSegment(castType: null), "castType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_CastTypeName()
        {
            Assert.ThrowsArgumentNull(() => new ComplexCastPathSegment(castTypeName: null), "castTypeName");
        }

        [Fact]
        public void GetEdmType_ReturnsCastType_IfPreviousTypeIsNotCollection()
        {
            // Arrange
            EdmComplexType castType = new EdmComplexType("NS", "Complex");
            EdmComplexType previousEdmType = new EdmComplexType("NS", "PreviousType");
            ComplexCastPathSegment castSegment = new ComplexCastPathSegment(castType);

            // Act
            var result = castSegment.GetEdmType(previousEdmType);

            // Assert
            Assert.Equal(castType, result);
        }

        [Fact]
        public void GetEdmType_ReturnsCollectionCastType_IfPreviousTypeIsCollection()
        {
            // Arrange
            EdmComplexType castType = new EdmComplexType("NS", "Complex");
            EdmCollectionType previousEdmType =
                new EdmCollectionType(new EdmComplexType("NS", "PreviousType").AsReference());
            ComplexCastPathSegment castSegment = new ComplexCastPathSegment(castType);

            // Act
            var result = castSegment.GetEdmType(previousEdmType);

            // Assert
            Assert.Equal(EdmTypeKind.Collection, result.TypeKind);
            Assert.Equal(castType, (result as IEdmCollectionType).ElementType.Definition);
        }

        [Fact]
        public void GetNavigationSource_Returns_PreviousEntitySet()
        {
            // Arrange
            EdmComplexType castType = new EdmComplexType("NS", "Complex");
            ComplexCastPathSegment castSegment = new ComplexCastPathSegment(castType);
            IEdmNavigationSource previousNavigationSource = new Mock<IEdmNavigationSource>().Object;

            // Act
            var result = castSegment.GetNavigationSource(previousNavigationSource);

            // Assert
            Assert.Same(previousNavigationSource, result);
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfCastTypeMatch()
        {
            // Arrange
            EdmComplexType castType = new EdmComplexType("NS", "Complex");
            ComplexCastPathSegment castSegment = new ComplexCastPathSegment(castType);
            ComplexCastPathSegment pathSegment = new ComplexCastPathSegment(castType);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act
            var result = castSegment.TryMatch(pathSegment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }
    }
}
