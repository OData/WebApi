// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class MutableObjectModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_NoPrefixInValueProvider_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_PrefixInValueProvider_ComplexType_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => new MutableTestType(), typeof(MutableTestType)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<MutableObjectModelBinder>(binder);
        }

        [Fact]
        public void GetBinder_PrefixInValueProvider_SimpleType_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeIsComplexModelDto_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ComplexModelDto)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        class MutableTestType
        {
        }
    }
}
