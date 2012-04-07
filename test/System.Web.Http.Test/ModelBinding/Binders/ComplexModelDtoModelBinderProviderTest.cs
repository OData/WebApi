// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Metadata.Providers;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class ComplexModelDtoModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_TypeDoesNotMatch_ReturnsNull()
        {
            // Arrange
            ComplexModelDtoModelBinderProvider provider = new ComplexModelDtoModelBinderProvider();
            ModelBindingContext bindingContext = GetBindingContext(typeof(object));

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_TypeMatches_ReturnsBinder()
        {
            // Arrange
            ComplexModelDtoModelBinderProvider provider = new ComplexModelDtoModelBinderProvider();
            ModelBindingContext bindingContext = GetBindingContext(typeof(ComplexModelDto));

            // Act
            IModelBinder binder = provider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<ComplexModelDtoModelBinder>(binder);
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
