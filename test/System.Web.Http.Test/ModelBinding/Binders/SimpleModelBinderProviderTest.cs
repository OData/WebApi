// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.ModelBinding.Binders
{
    public class SimpleModelBinderProviderTest
    {
        [Fact]
        public void ConstructorWithFactoryThrowsIfModelBinderFactoryIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new SimpleModelBinderProvider(typeof(object), (Func<IModelBinder>)null),
                "modelBinderFactory");
        }

        [Fact]
        public void ConstructorWithFactoryThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new SimpleModelBinderProvider(null, () => null),
                "modelType");
        }

        [Fact]
        public void ConstructorWithInstanceThrowsIfModelBinderIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new SimpleModelBinderProvider(typeof(object), (IModelBinder)null),
                "modelBinder");
        }

        [Fact]
        public void ConstructorWithInstanceThrowsIfModelTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                () => new SimpleModelBinderProvider(null, new Mock<IModelBinder>().Object),
                "modelType");
        }

        [Fact]
        public void GetBinder_TypeDoesNotMatch_ReturnsNull()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IModelBinder>().Object)
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
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), binderInstance);

            ModelBindingContext bindingContext = GetBindingContext(typeof(string));
            bindingContext.ValueProvider = new SimpleHttpValueProvider();

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixSuppressed_ReturnsFactoryInstance()
        {
            // Arrange
            int numExecutions = 0;
            IModelBinder theBinderInstance = new Mock<IModelBinder>().Object;
            Func<IModelBinder> factory = delegate
            {
                numExecutions++;
                return theBinderInstance;
            };

            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), factory)
            {
                SuppressPrefixCheck = true
            };
            ModelBindingContext bindingContext = GetBindingContext(typeof(string));

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);
            returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Equal(2, numExecutions);
            Assert.Equal(theBinderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinder_TypeMatches_PrefixSuppressed_ReturnsInstance()
        {
            // Arrange
            IModelBinder theBinderInstance = new Mock<IModelBinder>().Object;
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), theBinderInstance)
            {
                SuppressPrefixCheck = true
            };
            ModelBindingContext bindingContext = GetBindingContext(typeof(string));

            // Act
            IModelBinder returnedBinder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Equal(theBinderInstance, returnedBinder);
        }

        [Fact]
        public void GetBinderThrowsIfBindingContextIsNull()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IModelBinder>().Object);

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { provider.GetBinder(null, null); }, "bindingContext");
        }

        [Fact]
        public void ModelTypeProperty()
        {
            // Arrange
            SimpleModelBinderProvider provider = new SimpleModelBinderProvider(typeof(string), new Mock<IModelBinder>().Object);

            // Act & assert
            Assert.Equal(typeof(string), provider.ModelType);
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
