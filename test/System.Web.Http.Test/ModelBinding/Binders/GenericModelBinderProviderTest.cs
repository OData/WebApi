// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ModelBinding.Binders
{
    public class GenericModelBinderProviderTest
    {
        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelBinderFactoryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(typeof(List<>), (Func<Type[], IModelBinder>)null),
                "modelBinderFactory");
        }

        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                () => new GenericModelBinderProvider(typeof(List<int>), _ => null),
                @"The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type.
Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithFactory_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(null, _ => null),
                "modelType");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelBinderIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(typeof(List<>), (IModelBinder)null),
                "modelBinder");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                () => new GenericModelBinderProvider(typeof(List<int>), new MutableObjectModelBinder()),
                @"The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type.
Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithInstance_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(null, new MutableObjectModelBinder()),
                "modelType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeIsNotModelBinder()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                () => new GenericModelBinderProvider(typeof(List<>), typeof(string)),
                @"The type 'System.String' does not implement the interface 'System.Web.Http.ModelBinding.IModelBinder'.
Parameter name: modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(typeof(List<>), (Type)null),
                "modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelBinderTypeTypeArgumentMismatch()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                () => new GenericModelBinderProvider(typeof(List<>), typeof(DictionaryModelBinder<,>)),
                @"The open model type 'System.Collections.Generic.List`1[T]' has 1 generic type argument(s), but the open binder type 'System.Web.Http.ModelBinding.Binders.DictionaryModelBinder`2[TKey,TValue]' has 2 generic type argument(s). The binder type must not be an open generic type or must have the same number of generic arguments as the open model type.
Parameter name: modelBinderType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelTypeIsNotOpenGeneric()
        {
            // Act & assert
            Assert.Throws<ArgumentException>(
                () => new GenericModelBinderProvider(typeof(List<int>), typeof(MutableObjectModelBinder)),
                @"The type 'System.Collections.Generic.List`1[System.Int32]' is not an open generic type.
Parameter name: modelType");
        }

        [Fact]
        public void Constructor_WithType_ThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new GenericModelBinderProvider(null, typeof(MutableObjectModelBinder)),
                "modelType");
        }

        [Fact]
        public void GetBinder_TypeDoesNotMatch_ModelTypeIsInterface_ReturnsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IEnumerable<>), typeof(CollectionModelBinder<>))
            {
                SuppressPrefixCheck = true
            };
            ModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeDoesNotMatch_ModelTypeIsNotInterface_ReturnsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), typeof(CollectionModelBinder<>))
            {
                SuppressPrefixCheck = true
            };
            ModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixNotFound_ReturnsNull()
        {
            // Arrange
            IModelBinder binderInstance = new Mock<IModelBinder>().Object;
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), binderInstance);

            ModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));
            bindingContext.ValueProvider = new SimpleHttpValueProvider();

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_Factory_ReturnsBinder()
        {
            // Arrange
            IModelBinder binderInstance = new Mock<IModelBinder>().Object;

            Func<Type[], IModelBinder> binderFactory = typeArguments =>
            {
                Assert.Equal(new[] { typeof(int) }, typeArguments);
                return binderInstance;
            };

            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IList<>), binderFactory)
            {
                SuppressPrefixCheck = true
            };

            ModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Same(binderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_Instance_ReturnsBinder()
        {
            // Arrange
            IModelBinder binderInstance = new Mock<IModelBinder>().Object;

            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), binderInstance)
            {
                SuppressPrefixCheck = true
            };

            ModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Same(binderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_Success_TypeActivation_ReturnsBinder()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(List<>), typeof(CollectionModelBinder<>))
            {
                SuppressPrefixCheck = true
            };

            ModelBindingContext bindingContext = GetBindingContext(typeof(List<int>));

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<CollectionModelBinder<int>>(returnedBinder);
        }

        [Fact]
        public void GetBinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            GenericModelBinderProvider provider = new GenericModelBinderProvider(typeof(IEnumerable<>), typeof(CollectionModelBinder<>));

            // Act & assert
            Assert.ThrowsArgumentNull(
                () => provider.GetBinder(null, null),
                "bindingContext");
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => null, modelType)
            };
        }
    }
}
