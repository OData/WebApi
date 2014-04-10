// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class CastPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_CastType()
        {
            Assert.ThrowsArgumentNull(() => new CastPathSegment(castType: null), "castType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_CastTypeName()
        {
            Assert.ThrowsArgumentNull(() => new CastPathSegment(castTypeName: null), "castTypeName");
        }

        [Fact]
        public void GetEdmType_ReturnsCastType_IfPreviousTypeIsNotCollection()
        {
            // Arrange
            EdmEntityType castType = new EdmEntityType("NS", "Entity");
            EdmEntityType previousEdmType = new EdmEntityType("NS", "PreviousType");
            CastPathSegment castSegment = new CastPathSegment(castType);

            // Act
            var result = castSegment.GetEdmType(previousEdmType);

            // Assert
            Assert.Equal(castType, result);
        }

        [Fact]
        public void GetEdmType_ReturnsCollectionCastType_IfPreviousTypeIsCollection()
        {
            // Arrange
            EdmEntityType castType = new EdmEntityType("NS", "Entity");
            EdmCollectionType previousEdmType = new EdmCollectionType(new EdmEntityType("NS", "PreviousType").AsReference());
            CastPathSegment castSegment = new CastPathSegment(castType);

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
            EdmEntityType castType = new EdmEntityType("NS", "Entity");
            CastPathSegment castSegment = new CastPathSegment(castType);
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
            EdmEntityType castType = new EdmEntityType("NS", "Entity");
            CastPathSegment castSegment = new CastPathSegment(castType);
            CastPathSegment pathSegment = new CastPathSegment(castType);
            Dictionary<string, object> values = new Dictionary<string,object>();

            // Act
            var result = castSegment.TryMatch(pathSegment, values);

            // Assert
            Assert.True(result);
            Assert.Empty(values);
        }
    }
}
