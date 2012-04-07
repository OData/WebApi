// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Validation
{
    public class ModelValidatorTest
    {
        private static CachedDataAnnotationsModelMetadataProvider _metadataProvider = new CachedDataAnnotationsModelMetadataProvider();
        private static IEnumerable<ModelValidatorProvider> _noValidatorProviders = Enumerable.Empty<ModelValidatorProvider>();

        [Fact]
        public void ConstructorGuards()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForType(null, typeof(object));

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new TestableModelValidator(metadata: null, validatorProviders: _noValidatorProviders),
                "metadata");
            Assert.ThrowsArgumentNull(
                () => new TestableModelValidator(metadata, validatorProviders: null),
                "validatorProviders");
        }

        [Fact]
        public void ValuesSet()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(() => 15, typeof(string), "Length");

            // Act
            TestableModelValidator validator = new TestableModelValidator(metadata, _noValidatorProviders);

            // Assert
            Assert.Same(metadata, validator.Metadata);
            Assert.Same(_noValidatorProviders, validator.ValidatorProviders);
        }

        [Fact]
        public void IsRequiredFalseByDefault()
        {
            // Arrange
            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(() => 15, typeof(string), "Length");

            // Act
            TestableModelValidator validator = new TestableModelValidator(metadata, _noValidatorProviders);

            // Assert
            Assert.False(validator.IsRequired);
        }

        [Fact]
        public void GetModelValidator_DoesNotReadPropertyValues()
        {
            // Arrange
            IEnumerable<ModelValidatorProvider> validatorProviders = new[] { new ObservableModelValidatorProvider() };
            ObservableModel model = new ObservableModel();
            ModelMetadata metadata = new EmptyModelMetadataProvider().GetMetadataForType(() => model, typeof(ObservableModel));

            // Act
            ModelValidator validator = ModelValidator.GetModelValidator(metadata, validatorProviders);
            ModelValidationResult[] results = validator.Validate(model).ToArray();

            // Assert
            Assert.False(model.PropertyWasRead());
        }

        private class ObservableModelValidatorProvider : ModelValidatorProvider
        {
            public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
            {
                return new ModelValidator[] { new ObservableModelValidator(metadata, validatorProviders) };
            }

            private class ObservableModelValidator : ModelValidator
            {
                public ObservableModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
                    : base(metadata, validatorProviders)
                {
                }

                public override IEnumerable<ModelValidationResult> Validate(object container)
                {
                    return Enumerable.Empty<ModelValidationResult>();
                }
            }
        }

        private class ObservableModel
        {
            private bool _propertyWasRead;

            public int TheProperty
            {
                get
                {
                    _propertyWasRead = true;
                    return 42;
                }
            }

            public bool PropertyWasRead()
            {
                return _propertyWasRead;
            }
        }

        private class TestableModelValidator : ModelValidator
        {
            public TestableModelValidator(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders)
                : base(metadata, validatorProviders)
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                throw new NotImplementedException();
            }
        }
    }
}
