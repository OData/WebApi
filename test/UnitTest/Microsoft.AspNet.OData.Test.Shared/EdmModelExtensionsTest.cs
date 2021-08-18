//-----------------------------------------------------------------------------
// <copyright file="EdmModelExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class EdmModelExtensionsTest
    {
        [Fact]
        public void GetNavigationSourceLinkBuilder_ThrowsArgumentNull_Model()
        {
            IEdmModel model = null;
            ExceptionAssert.ThrowsArgumentNull(
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
        public void GetOperationLinkBuilder_ThrowsArgumentNull_Model()
        {
            // Arrange
            IEdmModel model = null;
            IEdmFunction function = new Mock<IEdmFunction>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => model.GetOperationLinkBuilder(function), "model");
        }

        [Fact]
        public void GetOperationLinkBuilder_ThrowsArgumentNull_Operation()
        {
            // Arrange
            IEdmModel model = new EdmModel();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => model.GetOperationLinkBuilder(operation: null), "operation");
        }

        [Fact]
        public void GetFunctionOperationLinkBuilder_After_SetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmFunction function = new Mock<IEdmFunction>().Object;
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceContext _) => null, followsConventions: false);

            // Act
            model.SetOperationLinkBuilder(function, builder);
            var result = model.GetOperationLinkBuilder(function);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetFunctionOperationLinkBuilderForFeed_After_SetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmFunction function = new Mock<IEdmFunction>().Object;
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceSetContext _) => null, followsConventions: false);

            // Act
            model.SetOperationLinkBuilder(function, builder);
            var result = model.GetOperationLinkBuilder(function);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetFunctionOperationLinkBuilder_ReturnsDefaultOperationLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType);
            function.AddParameter("entity", new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false));

            // Act
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(function);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.LinkFactory);
            Assert.IsType<Func<ResourceContext, Uri>>(builder.LinkFactory);

            Assert.Null(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetFunctionOperationLinkBuilderForFeed_ReturnsDefaultOperationLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType);
            function.AddParameter("entityset",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false))));

            // Act
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(function);

            // Assert
            Assert.NotNull(builder);
            Assert.Null(builder.LinkFactory);

            Assert.NotNull(builder.FeedLinkFactory);
            Assert.IsType<Func<ResourceSetContext, Uri>>(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetActionOperationLinkBuilder_After_SetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmAction action = new Mock<IEdmAction>().Object;
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceContext _) => null, followsConventions: false);

            // Act
            model.SetOperationLinkBuilder(action, builder);
            var result = model.GetOperationLinkBuilder(action);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetActionOperationLinkBuilderForFeed_After_SetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmAction action = new Mock<IEdmAction>().Object;
            OperationLinkBuilder builder = new OperationLinkBuilder((ResourceSetContext _) => null, followsConventions: false);

            // Act
            model.SetOperationLinkBuilder(action, builder);
            var result = model.GetOperationLinkBuilder(action);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void GetActionOperationLinkBuilder_ReturnsDefaultOperationLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            EdmAction action = new EdmAction("NS", "Action", returnType: null);
            action.AddParameter("entity", new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false));

            // Act
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(action);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.LinkFactory);
            Assert.IsType<Func<ResourceContext, Uri>>(builder.LinkFactory);
            Assert.Null(builder.FeedLinkFactory);
        }

        [Fact]
        public void GetActionOperationLinkBuilderForFeed_ReturnsDefaultOperationLinkBuilder_IfNotSet()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            EdmAction action = new EdmAction("NS", "Action", returnType: null);
            action.AddParameter("entityset",
                new EdmCollectionTypeReference(
                    new EdmCollectionType(new EdmEntityTypeReference(new EdmEntityType("NS", "Customer"), false))));

            // Act
            OperationLinkBuilder builder = model.GetOperationLinkBuilder(action);

            // Assert
            Assert.NotNull(builder);
            Assert.Null(builder.LinkFactory);

            Assert.NotNull(builder.FeedLinkFactory);
            Assert.IsType<Func<ResourceSetContext, Uri>>(builder.FeedLinkFactory);
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
