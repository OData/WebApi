// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.TestUtil;
using Moq;
using Xunit;

namespace System.Web.Http.ModelBinding
{
    public class ModelBinderConfigTest
    {
        [Fact]
        public void GetUserResourceString_NullControllerContext_ReturnsNull()
        {
            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(null /* controllerContext */, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Null(customResourceString);
        }

        [Fact(Skip = "This functionality isn't enabled yet")]
        public void GetUserResourceString_NullHttpContext_ReturnsNull()
        {
            Mock<HttpActionContext> context = new Mock<HttpActionContext>();
            //context.Setup(o => o.HttpContext).Returns((HttpContextBase)null);

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(context.Object, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Null(customResourceString);
        }

        [Fact(Skip = "This functionality isn't enabled yet")]
        public void GetUserResourceString_NullResourceKey_ReturnsNull()
        {
            Mock<HttpActionContext> context = new Mock<HttpActionContext>();

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(context.Object, "someResourceName", null /* resourceClassKey */);

            // Assert
            //context.Verify(o => o.HttpContext, Times.Never());
            Assert.Null(customResourceString);
        }

        [Fact(Skip = "This functionality isn't enabled yet")]
        public void GetUserResourceString_ValidResourceObject_ReturnsResourceString()
        {
            Mock<HttpActionContext> context = new Mock<HttpActionContext>();
            //context.Setup(o => o.HttpContext.GetGlobalResourceObject("someResourceClassKey", "someResourceName", CultureInfo.CurrentUICulture))
            //       .Returns("My custom resource string");

            // Act
            string customResourceString = ModelBinderConfig.GetUserResourceString(context.Object, "someResourceName", "someResourceClassKey");

            // Assert
            Assert.Equal("My custom resource string", customResourceString);
        }

        [Fact]
        public void TypeConversionErrorMessageProvider_DefaultValue()
        {
            // Arrange
            ModelMetadata metadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(int), "SomePropertyName");

            // Act
            string errorString = ModelBinderConfig.TypeConversionErrorMessageProvider(null, metadata, "some incoming value");

            // Assert
            Assert.Equal("The value 'some incoming value' is not valid for SomePropertyName.", errorString);
        }

        [Fact]
        public void TypeConversionErrorMessageProvider_Property()
        {
            // Arrange
            ModelBinderConfigWrapper wrapper = new ModelBinderConfigWrapper();

            // Act & assert
            try
            {
                MemberHelper.TestPropertyWithDefaultInstance(wrapper, "TypeConversionErrorMessageProvider", (ModelBinderErrorMessageProvider)DummyErrorSelector);
            }
            finally
            {
                wrapper.Reset();
            }
        }

        [Fact]
        public void ValueRequiredErrorMessageProvider_DefaultValue()
        {
            // Arrange
            ModelMetadata metadata = new ModelMetadata(new Mock<ModelMetadataProvider>().Object, null, null, typeof(int), "SomePropertyName");

            // Act
            string errorString = ModelBinderConfig.ValueRequiredErrorMessageProvider(null, metadata, "some incoming value");

            // Assert
            Assert.Equal("A value is required.", errorString);
        }

        [Fact]
        public void ValueRequiredErrorMessageProvider_Property()
        {
            // Arrange
            ModelBinderConfigWrapper wrapper = new ModelBinderConfigWrapper();

            // Act & assert
            try
            {
                MemberHelper.TestPropertyWithDefaultInstance(wrapper, "ValueRequiredErrorMessageProvider", (ModelBinderErrorMessageProvider)DummyErrorSelector);
            }
            finally
            {
                wrapper.Reset();
            }
        }

        private string DummyErrorSelector(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue)
        {
            throw new NotImplementedException();
        }

        private sealed class ModelBinderConfigWrapper
        {
            public ModelBinderErrorMessageProvider TypeConversionErrorMessageProvider
            {
                get { return ModelBinderConfig.TypeConversionErrorMessageProvider; }
                set { ModelBinderConfig.TypeConversionErrorMessageProvider = value; }
            }

            public ModelBinderErrorMessageProvider ValueRequiredErrorMessageProvider
            {
                get { return ModelBinderConfig.ValueRequiredErrorMessageProvider; }
                set { ModelBinderConfig.ValueRequiredErrorMessageProvider = value; }
            }

            public void Reset()
            {
                ModelBinderConfig.TypeConversionErrorMessageProvider = null;
                ModelBinderConfig.ValueRequiredErrorMessageProvider = null;
            }
        }
    }
}
