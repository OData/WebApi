// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public void GetActionLinkBuilder_ThrowsArgumentNull_Action()
        {
            // Arrange
            IEdmModel model = new EdmModel();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => model.GetActionLinkBuilder(action: null), "action");
        }

        [Fact]
        public void GetActionLinkBuilder_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmAction action = new Mock<IEdmAction>().Object;
            ActionLinkBuilder builder = new ActionLinkBuilder((EntityInstanceContext _)=> null, followsConventions: false);

            // Act
            model.SetActionLinkBuilder(action, builder);
            var result = model.GetActionLinkBuilder(action);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetActionLinkBuilderForFeed_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmAction action = new Mock<IEdmAction>().Object;
            ActionLinkBuilder builder = new ActionLinkBuilder((FeedContext _) => null, followsConventions: false);

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
            EdmAction action = new EdmAction("NS", "Action", returnType: null);
            action.AddParameter("entity", new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false));

            // Act
            ActionLinkBuilder builder = model.GetActionLinkBuilder(action);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.LinkFactory);
            Assert.IsType<Func<EntityInstanceContext, Uri>>(builder.LinkFactory);
            Assert.Null(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetActionLinkBuilderForFeed_ReturnsDefaultActionLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmAction action = new EdmAction("NS", "Action", returnType: null);
            action.AddParameter("entityset",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false))));

            // Act
            ActionLinkBuilder builder = model.GetActionLinkBuilder(action);

            // Assert
            Assert.NotNull(builder);
            Assert.Null(builder.LinkFactory);

            Assert.NotNull(builder.FeedLinkFactory);
            Assert.IsType<Func<FeedContext, Uri>>(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetFunctionLinkBuilder_ThrowsArgumentNull_Model()
        {
            // Arrange
            IEdmModel model = null;
            IEdmFunction function = new Mock<IEdmFunction>().Object;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => model.GetFunctionLinkBuilder(function), "model");
        }

        [Fact]
        public void GetFunctionLinkBuilder_ThrowsArgumentNull_Function()
        {
            // Arrange
            IEdmModel model = new EdmModel();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => model.GetFunctionLinkBuilder(function: null), "function");
        }

        [Fact]
        public void GetFunctionLinkBuilder_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmFunction function = new Mock<IEdmFunction>().Object;
            FunctionLinkBuilder builder = new FunctionLinkBuilder((EntityInstanceContext _) => null, followsConventions: false);

            // Act
            model.SetFunctionLinkBuilder(function, builder);
            var result = model.GetFunctionLinkBuilder(function);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetFunctionLinkBuilderForFeed_After_SetActionLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmFunction function = new Mock<IEdmFunction>().Object;
            FunctionLinkBuilder builder = new FunctionLinkBuilder((FeedContext _) => null, followsConventions: false);

            // Act
            model.SetFunctionLinkBuilder(function, builder);
            var result = model.GetFunctionLinkBuilder(function);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetFunctionLinkBuilder_ReturnsDefaultActionLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType);
            function.AddParameter("entity", new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false));

            // Act
            FunctionLinkBuilder builder = model.GetFunctionLinkBuilder(function);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.LinkFactory);
            Assert.IsType<Func<EntityInstanceContext, Uri>>(builder.LinkFactory);

            Assert.Null(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetFunctionLinkBuilderForFeed_ReturnsDefaultActionLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType);
            function.AddParameter("entityset",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false))));

            // Act
            FunctionLinkBuilder builder = model.GetFunctionLinkBuilder(function);

            // Assert
            Assert.NotNull(builder);
            Assert.Null(builder.LinkFactory);

            Assert.NotNull(builder.FeedLinkFactory);
            Assert.IsType<Func<FeedContext, Uri>>(builder.FeedLinkFactory);
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
