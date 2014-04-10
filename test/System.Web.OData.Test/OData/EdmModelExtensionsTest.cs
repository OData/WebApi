// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData
{
    public class EdmModelExtensionsTest
    {
        [Fact]
        public void GetNavigationSourceLinkBuilder_ThrowsArgumentNull_Model()
        {
            IEdmModel model = null;
            Assert.ThrowsArgumentNull(
                () => model.GetNavigationSourceLinkBuilder(navigationSource: new Mock<IEdmNavigationSource>().Object),
                "model");
        }

        [Fact]
        public void GetNavigationSourceLinkBuilder_After_SetNavigationSourceLinkBuilder_OnEntitySet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmEntitySet entitySet = new EdmEntitySet(container, "EntitySet", entityType);
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation();

            // Act
            model.SetNavigationSourceLinkBuilder(entitySet, linkBuilder);
            var result = model.GetNavigationSourceLinkBuilder(entitySet);

            // Assert
            Assert.Same(linkBuilder, result);
        }

        [Fact]
        public void GetNavigationSourceLinkBuilder_After_SetNavigationSourceLinkBuilder_OnSingleton()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmSingleton singleton = new EdmSingleton(container, "Singleton", entityType);
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation();

            // Act
            model.SetNavigationSourceLinkBuilder(singleton, linkBuilder);
            var result = model.GetNavigationSourceLinkBuilder(singleton);

            // Assert
            Assert.Same(linkBuilder, result);
        }

        [Fact]
        public void GetNavigationSourceLinkBuilder_ReturnsDefaultNavigationSourceBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmNavigationSource navigationSource = new EdmEntitySet(container, "EntitySet", entityType);

            // Act & Assert
            Assert.NotNull(model.GetNavigationSourceLinkBuilder(navigationSource));
        }

        [Fact]
        public void GetActionLinkBuilder_ThrowsArgumentNull_Model()
        {
            // Arrange
            IEdmModel model = null;
            IEdmAction action = new Mock<IEdmAction>().Object;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => model.GetActionLinkBuilder(action), "model");
        }

        [Fact]
        public void GetActionLinkBuilder_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            IEdmAction action = new Mock<IEdmAction>().Object;
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
            IEdmAction action = new EdmAction("NS", "Action", returnType: null);

            // Act & Assert
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
            // Arrange
            IEdmModel model = new EdmModel();

            // Act
            ClrTypeCache cache1 = model.GetTypeMappingCache();
            ClrTypeCache cache2 = model.GetTypeMappingCache();

            // Assert
            Assert.Same(cache1, cache2);
        }
    }
}
