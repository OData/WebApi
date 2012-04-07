// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Util;
using Xunit;

namespace System.Web.Http.ModelBinding
{
    public class CollectionModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IEnumerable<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<CollectionModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsIncorrect_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainPrefix_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IEnumerable<int>)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider()
            };

            CollectionModelBinderProvider binderProvider = new CollectionModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }
    }
}
