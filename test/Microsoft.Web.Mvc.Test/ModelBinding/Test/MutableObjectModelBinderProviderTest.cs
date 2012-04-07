// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.Web.UnitTestUtil;
using Xunit;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class MutableObjectModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_NoPrefixInValueProvider_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_PrefixInValueProvider_ReturnsBinder()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(() => 42, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<MutableObjectModelBinder>(binder);
        }

        [Fact]
        public void GetBinder_TypeIsComplexModelDto_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(ComplexModelDto)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo.bar", "someValue" }
                }
            };

            MutableObjectModelBinderProvider binderProvider = new MutableObjectModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }
    }
}
