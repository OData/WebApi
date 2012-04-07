// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class TypeMatchModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_InvalidValueProviderResult_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", "not an integer" }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void BindModel_ValidValueProviderResult_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext();
            bindingContext.ValueProvider = new SimpleHttpValueProvider
            {
                { "theModelName", 42 }
            };

            TypeMatchModelBinderProvider provider = new TypeMatchModelBinderProvider();

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<TypeMatchModelBinder>(binder);
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
