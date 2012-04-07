// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Moq;
using Moq.Protected;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class DataAnnotationsModelValidatorTest
    {
        [Fact]
        public void ConstructorGuards()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(null, context, attribute),
                "metadata");
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(metadata, null, attribute),
                "controllerContext");
            Assert.ThrowsArgumentNull(
                () => new DataAnnotationsModelValidator(metadata, context, null),
                "attribute");
        }

        [Fact]
        public void ValuesSet()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.Same(attribute, validator.Attribute);
            Assert.Equal(attribute.FormatErrorMessage("Length"), validator.ErrorMessage);
        }

        [Fact]
        public void NoClientRulesByDefault()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            RequiredAttribute attribute = new RequiredAttribute();

            // Act
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute);

            // Assert
            Assert.Empty(validator.GetClientValidationRules());
        }

        [Fact]
        public void ValidateWithIsValidTrue()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(true);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ValidateWithIsValidFalse()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Setup(a => a.IsValid(metadata.Model)).Returns(false);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            var validationResult = result.Single();
            Assert.Equal("", validationResult.MemberName);
            Assert.Equal(attribute.Object.FormatErrorMessage("Length"), validationResult.Message);
        }

        [Fact]
        public void ValidatateWithValidationResultSuccess()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();
            Mock<ValidationAttribute> attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.Protected()
                .Setup<ValidationResult>("IsValid", ItExpr.IsAny<object>(), ItExpr.IsAny<ValidationContext>())
                .Returns(ValidationResult.Success);
            DataAnnotationsModelValidator validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            IEnumerable<ModelValidationResult> result = validator.Validate(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void IsRequiredTests()
        {
            // Arrange
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => 15, typeof(string), "Length");
            ControllerContext context = new ControllerContext();

            // Act & Assert
            Assert.False(new DataAnnotationsModelValidator(metadata, context, new RangeAttribute(10, 20)).IsRequired);
            Assert.True(new DataAnnotationsModelValidator(metadata, context, new RequiredAttribute()).IsRequired);
            Assert.True(new DataAnnotationsModelValidator(metadata, context, new DerivedRequiredAttribute()).IsRequired);
        }

        class DerivedRequiredAttribute : RequiredAttribute
        {
        }

        [Fact]
        public void AttributeWithIClientValidatableGetsClientValidationRules()
        {
            // Arrange
            var expected = new ModelClientValidationStringLengthRule("Error", 1, 10);
            var context = new ControllerContext();
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(string));
            var attribute = new Mock<ValidationAttribute> { CallBase = true };
            attribute.As<IClientValidatable>()
                .Setup(cv => cv.GetClientValidationRules(metadata, context))
                .Returns(new[] { expected })
                .Verifiable();
            var validator = new DataAnnotationsModelValidator(metadata, context, attribute.Object);

            // Act
            ModelClientValidationRule actual = validator.GetClientValidationRules().Single();

            // Assert
            attribute.Verify();
            Assert.Same(expected, actual);
        }
    }
}
