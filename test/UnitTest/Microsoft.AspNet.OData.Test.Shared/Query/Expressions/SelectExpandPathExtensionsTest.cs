// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class SelectExpandPathExtensionsTest
    {
        [Fact]
        public void GetFirstNonTypeCastSegment_SelectPath_ThrowsArgumentNull()
        {
            // Arrange & Act
            ODataSelectPath selectPath = null;
            IList<ODataPathSegment> remainingSegments;

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => selectPath.GetFirstNonTypeCastSegment(out remainingSegments), "selectPath");
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_ExpandPath_ThrowsArgumentNull()
        {
            // Arrange & Act
            ODataExpandPath expandPath = null;
            IList<ODataPathSegment> remainingSegments;

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => expandPath.GetFirstNonTypeCastSegment(out remainingSegments), "expandPath");
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_ThrowsForUnsupportedMiddleSegmentInSelectPath()
        {
            // Arrange & Act
            ODataSelectPath selectPath = new ODataSelectPath(new DynamicPathSegment("abc"), new DynamicPathSegment("xyz"));
            IList<ODataPathSegment> remainingSegments;

            // Assert
            ExceptionAssert.Throws<ODataException>(() => selectPath.GetFirstNonTypeCastSegment(out remainingSegments),
                String.Format(SRResources.InvalidSegmentInSelectExpandPath, "DynamicPathSegment"));
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_ThrowsForUnsupportedLastSegmentInSelectPath()
        {
            // Arrange & Act
            IEdmType stringType = EdmCoreModel.Instance.GetString(false).Definition;
            ODataSelectPath selectPath = new ODataSelectPath(new TypeSegment(stringType, null), new TypeSegment(stringType, null));
            IList<ODataPathSegment> remainingSegments;

            // Assert
            ExceptionAssert.Throws<ODataException>(() => selectPath.GetFirstNonTypeCastSegment(out remainingSegments),
                String.Format(SRResources.InvalidLastSegmentInSelectExpandPath, "TypeSegment"));
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_WorksForSelectPathWithFirstNonTypeSegmentAtBegin()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String);

            ODataPathSegment firstPropertySegment = new PropertySegment(property);
            ODataPathSegment secondPropertySegment = new PropertySegment(property);
            ODataSelectPath selectPath = new ODataSelectPath(firstPropertySegment, secondPropertySegment);

            // Act
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = selectPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // Assert
            Assert.NotNull(firstNonTypeSegment);
            Assert.Same(firstPropertySegment, firstNonTypeSegment);

            Assert.NotNull(remainingSegments);
            Assert.Same(secondPropertySegment, Assert.Single(remainingSegments));
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_WorksForSelectPathWithFirstNonTypeSegmentAtMiddle()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String);

            ODataPathSegment firstTypeSegment = new TypeSegment(entityType, null);
            ODataPathSegment secondTypeSegment = new TypeSegment(entityType, null);
            ODataPathSegment firstPropertySegment = new PropertySegment(property);
            ODataPathSegment secondPropertySegment = new PropertySegment(property);
            ODataSelectPath selectPath = new ODataSelectPath(
                firstTypeSegment,
                secondTypeSegment,
                firstPropertySegment, // here
                secondPropertySegment);

            // Act
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = selectPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // Assert
            Assert.NotNull(firstNonTypeSegment);
            Assert.Same(firstPropertySegment, firstNonTypeSegment);

            Assert.NotNull(remainingSegments);
            Assert.Same(secondPropertySegment, Assert.Single(remainingSegments));
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_WorksForSelectPathWithFirstNonTypeSegmentAtLast()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String);

            ODataPathSegment firstTypeSegment = new TypeSegment(entityType, null);
            ODataPathSegment secondTypeSegment = new TypeSegment(entityType, null);
            ODataPathSegment firstPropertySegment = new PropertySegment(property);
            ODataSelectPath selectPath = new ODataSelectPath(
                firstTypeSegment,
                secondTypeSegment,
                firstPropertySegment);

            // Act
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = selectPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // Assert
            Assert.NotNull(firstNonTypeSegment);
            Assert.Same(firstPropertySegment, firstNonTypeSegment);

            Assert.Null(remainingSegments);
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_WorksForExpandPathOnlyWithTypeAndNavigationSegment()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            var navProperty = entityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Nav",
                Target = entityType,
                TargetMultiplicity = EdmMultiplicity.One
            });
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntitySet set = new EdmEntitySet(container, "set", entityType);
            NavigationPropertySegment navSegment = new NavigationPropertySegment(navProperty, set);
            TypeSegment typeSegment = new TypeSegment(entityType, null);
            ODataExpandPath expandPath = new ODataExpandPath(typeSegment, navSegment);

            // Act
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = expandPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // Assert
            Assert.NotNull(firstNonTypeSegment);
            Assert.Same(navSegment, firstNonTypeSegment);

            Assert.Null(remainingSegments);
        }

        [Fact]
        public void GetFirstNonTypeCastSegment_WorksForExpandPathWithPropertySegment()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String);
            var navProperty = entityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Nav",
                Target = entityType,
                TargetMultiplicity = EdmMultiplicity.One
            });
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntitySet set = new EdmEntitySet(container, "set", entityType);
            NavigationPropertySegment navSegment = new NavigationPropertySegment(navProperty, set);
            TypeSegment typeSegment = new TypeSegment(entityType, null);
            ODataPathSegment propertySegment = new PropertySegment(property);
            ODataExpandPath expandPath = new ODataExpandPath(typeSegment, propertySegment, navSegment);

            // Act
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = expandPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // Assert
            Assert.NotNull(firstNonTypeSegment);
            Assert.Same(propertySegment, firstNonTypeSegment);

            Assert.NotNull(remainingSegments);
            Assert.Same(navSegment, Assert.Single(remainingSegments));
        }
    }
}
