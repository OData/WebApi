// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data.Linq;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.Util;
using Xunit;

namespace System.Web.Http.ModelBinding.Binders
{
    public class BinaryDataModelBinderProviderTest
    {
        private static readonly byte[] _base64Bytes = new byte[] { 0x12, 0x20, 0x34, 0x40 };
        private const string _base64String = "EiA0QA==";

        [Fact]
        public void BindModel_BadValue_Fails()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo", "not base64 encoded!" }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void BindModel_EmptyValue_Fails()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo", "" }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void BindModel_GoodValue_ByteArray_Succeeds()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(_base64Bytes, (byte[])bindingContext.Model);
        }

        [Fact]
        public void BindModel_GoodValue_LinqBinary_Succeeds()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(Binary)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.True(retVal);
            Binary binaryModel = Assert.IsType<Binary>(bindingContext.Model);
            Assert.Equal(_base64Bytes, binaryModel.ToArray());
        }

        [Fact]
        public void BindModel_NoValue_Fails()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(byte[])),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo.bar", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder binder = binderProvider.GetBinder(null, bindingContext);
            bool retVal = binder.BindModel(null, bindingContext);

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void GetBinder_WrongModelType_ReturnsNull()
        {
            // Arrange
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(object)),
                ModelName = "foo",
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "foo", _base64String }
                }
            };

            BinaryDataModelBinderProvider binderProvider = new BinaryDataModelBinderProvider();

            // Act
            IModelBinder modelBinder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(modelBinder);
        }
    }
}
