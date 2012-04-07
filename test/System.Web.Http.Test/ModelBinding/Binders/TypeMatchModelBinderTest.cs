// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using System.Web.Http.ValueProviders;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeMatchModelBinderTest
    {
        [Fact]
        public void BindModel_InvalidValueProviderResult_ReturnsFalse()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeMatchModelBinder binder = new TypeMatchModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsTrue()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinder binder = new TypeMatchModelBinder();

            // Act
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(42, bindingContext.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("theModelName"));
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderResultRawValueIncorrect_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.Null(vpResult); // Raw value is the wrong type
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderResultValid_ReturnsValueProviderResult()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", 42 }
            };

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.NotNull(vpResult);
        }

        [Fact]
        public void GetCompatibleValueProviderResult_ValueProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider();

            // Act
            ValueProviderResult vpResult = TypeMatchModelBinder.GetCompatibleValueProviderResult(bindingContext);

            // Assert
            Assert.Null(vpResult); // No key matched
        }

        private static ModelBindingContext GetBindingContext()
        {
            return GetBindingContext(typeof(int));
        }

        private static ModelBindingContext GetBindingContext(Type modelType)
        {
            return new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, modelType),
                ModelName = "theModelName"
            };
        }
    }
}
