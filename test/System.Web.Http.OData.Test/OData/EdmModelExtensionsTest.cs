// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class EdmModelExtensionsTest
    {
        [Fact]
        public void GetEntitySetLinkBuilder_ThrowsArgumentNull_Model()
        {
            IEdmModel model = null;
            Assert.ThrowsArgumentNull(
                () => model.GetEntitySetLinkBuilder(entitySet: new Mock<IEdmEntitySet>().Object),
                "model");
        }

        [Fact]
        public void GetEntitySetLinkBuilder_After_SetEntitySetLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmEntitySet entitySet = new EdmEntitySet(container, "EntitySet", entityType);
            EntitySetLinkBuilderAnnotation entitySetLinkBuilder = new EntitySetLinkBuilderAnnotation();

            // Act
            model.SetEntitySetLinkBuilder(entitySet, entitySetLinkBuilder);
            var result = model.GetEntitySetLinkBuilder(entitySet);

            // Assert
            Assert.Same(entitySetLinkBuilder, result);
        }

        [Fact]
        public void GetEntitySetLinkBuilder_ReturnsDefaultEntitySetBuilder_IfNotSet()
        {
            IEdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmEntitySet entitySet = new EdmEntitySet(container, "EntitySet", entityType);

            Assert.NotNull(model.GetEntitySetLinkBuilder(entitySet));
        }

        [Fact]
        public void GetActionLinkBuilder_ThrowsArgumentNull_Model()
        {
            IEdmModel model = null;
            IEdmFunctionImport action = new Mock<IEdmFunctionImport>().Object;

            Assert.ThrowsArgumentNull(() => model.GetActionLinkBuilder(action), "model");
        }

        [Fact]
        public void GetActionLinkBuilder_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            IEdmFunctionImport action = new Mock<IEdmFunctionImport>().Object;
            ActionLinkBuilder builder = new ActionLinkBuilder(_=> null, followsConventions: false);

            // Act
            model.SetActionLinkBuilder(action, builder);
            var result = model.GetActionLinkBuilder(action);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetActionLinkBuilder_ReturnsDefaultActionLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            IEdmFunctionImport action = new EdmFunctionImport(container, "Action", returnType: null);

            Assert.NotNull(model.GetActionLinkBuilder(action));
        }

        [Fact]
        public void GetTypeMappingCache_ReturnsNewInstance_IfNotSet()
        {
            IEdmModel model = new EdmModel();
            Assert.NotNull(model.GetTypeMappingCache());
        }

        [Fact]
        public void GetTypeMappingCache_ReturnsCachedInstance_IfCalledMultipleTimes()
        {
            IEdmModel model = new EdmModel();
            ClrTypeCache cache1 = model.GetTypeMappingCache();
            ClrTypeCache cache2 = model.GetTypeMappingCache();

            Assert.Same(cache1, cache2);
        }
    }
}
