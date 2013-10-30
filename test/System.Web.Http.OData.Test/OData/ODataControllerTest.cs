// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class ODataControllerTest
    {
        [Fact]
        public void Validate_ThrowsInvalidOperationException_IfConfigurationIsNull()
        {
            // Arrange
            TestODataController controller = new TestODataController();
            TestEntity entity = new TestEntity();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => controller.Validate(entity),
                "ApiController.Configuration must not be null.");
        }

        [Fact]
        public void Validate_DoesNothing_IfValidatorIsNull()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IBodyModelValidator), null);
            TestEntity entity = new TestEntity { ID = 42 };

            TestODataController controller = new TestODataController { Configuration = configuration };

            // Act
            controller.Validate(entity);

            // Assert
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public void Validate_CallsValidateOnConfiguredValidator_UsingConfiguredMetadataProvider()
        {
            // Arrange
            Mock<IBodyModelValidator> validator = new Mock<IBodyModelValidator>();
            Mock<ModelMetadataProvider> metadataProvider = new Mock<ModelMetadataProvider>();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IBodyModelValidator), validator.Object);
            configuration.Services.Replace(typeof(ModelMetadataProvider), metadataProvider.Object);

            TestODataController controller = new TestODataController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = 42 };

            // Act
            controller.Validate(entity);

            // Assert
            validator.Verify(
                v => v.Validate(entity, typeof(TestEntity), metadataProvider.Object, controller.ActionContext, String.Empty),
                Times.Once());
            Assert.True(controller.ModelState.IsValid);
        }

        [Fact]
        public void Validate_SetsModelStateErrors_ForInvalidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestODataController controller = new TestODataController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = -1 };

            // Act
            controller.Validate(entity);

            // Assert
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("The field ID must be between 0 and 100.", controller.ModelState["ID"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_SetsModelStateErrorsUnderRightPrefix_ForInvalidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestODataController controller = new TestODataController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = -1 };

            // Act
            controller.Validate(entity, keyPrefix: "prefix");

            // Assert
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("The field ID must be between 0 and 100.",
                controller.ModelState["prefix.ID"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void Validate_DoesNotThrow_ForValidModels()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            TestODataController controller = new TestODataController { Configuration = configuration };
            TestEntity entity = new TestEntity { ID = 42 };

            // Act && Assert
            Assert.DoesNotThrow(() => controller.Validate(entity));
        }

        private class TestODataController : ODataController
        {
        }

        private class TestEntity
        {
            [Range(0, 100)]
            public int ID { get; set; }
        }
    }
}
