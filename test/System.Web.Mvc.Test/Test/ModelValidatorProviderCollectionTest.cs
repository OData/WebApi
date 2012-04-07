// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ModelValidatorProviderCollectionTest
    {
        [Fact]
        public void ListWrappingConstructor()
        {
            // Arrange
            List<ModelValidatorProvider> list = new List<ModelValidatorProvider>()
            {
                new Mock<ModelValidatorProvider>().Object, new Mock<ModelValidatorProvider>().Object
            };

            // Act
            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection(list);

            // Assert
            Assert.Equal(list, collection.ToList());
        }

        [Fact]
        public void ListWrappingConstructorThrowsIfListIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { new ModelValidatorProviderCollection((IList<ModelValidatorProvider>)null); },
                "list");
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Act
            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection();

            // Assert
            Assert.Empty(collection);
        }

        [Fact]
        public void AddNullModelValidatorProviderThrows()
        {
            // Arrange
            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection.Add(null); },
                "item");
        }

        [Fact]
        public void SetItem()
        {
            // Arrange
            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection();
            collection.Add(new Mock<ModelValidatorProvider>().Object);

            ModelValidatorProvider newProvider = new Mock<ModelValidatorProvider>().Object;

            // Act
            collection[0] = newProvider;

            // Assert
            ModelValidatorProvider provider = Assert.Single(collection);
            Assert.Equal(newProvider, provider);
        }

        [Fact]
        public void SetNullModelValidatorProviderThrows()
        {
            // Arrange
            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection();
            collection.Add(new Mock<ModelValidatorProvider>().Object);

            // Act & Assert
            Assert.ThrowsArgumentNull(
                delegate { collection[0] = null; },
                "item");
        }

        [Fact]
        public void GetValidators()
        {
            // Arrange
            ModelMetadata metadata = GetMetadata();
            ControllerContext controllerContext = new ControllerContext();

            ModelValidator[] allValidators = new ModelValidator[]
            {
                new SimpleModelValidator(),
                new SimpleModelValidator(),
                new SimpleModelValidator(),
                new SimpleModelValidator(),
                new SimpleModelValidator()
            };

            Mock<ModelValidatorProvider> provider1 = new Mock<ModelValidatorProvider>();
            provider1.Setup(p => p.GetValidators(metadata, controllerContext)).Returns(new ModelValidator[]
            {
                allValidators[0], allValidators[1]
            });

            Mock<ModelValidatorProvider> provider2 = new Mock<ModelValidatorProvider>();
            provider2.Setup(p => p.GetValidators(metadata, controllerContext)).Returns(new ModelValidator[]
            {
                allValidators[2], allValidators[3], allValidators[4]
            });

            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection();
            collection.Add(provider1.Object);
            collection.Add(provider2.Object);

            // Act
            IEnumerable<ModelValidator> returnedValidators = collection.GetValidators(metadata, controllerContext);

            // Assert
            Assert.Equal(allValidators, returnedValidators.ToArray());
        }

        [Fact]
        public void GetValidatorsDelegatesToResolver()
        {
            // Arrange
            ModelValidator[] allValidators = new ModelValidator[]
            {
                new SimpleModelValidator(),
                new SimpleModelValidator(),
                new SimpleModelValidator(),
                new SimpleModelValidator()
            };

            ModelMetadata metadata = GetMetadata();
            ControllerContext controllerContext = new ControllerContext();

            Mock<ModelValidatorProvider> resolverProvider1 = new Mock<ModelValidatorProvider>();
            resolverProvider1.Setup(p => p.GetValidators(metadata, controllerContext)).Returns(new ModelValidator[]
            {
                allValidators[0], allValidators[1]
            });

            Mock<ModelValidatorProvider> resolverprovider2 = new Mock<ModelValidatorProvider>();
            resolverprovider2.Setup(p => p.GetValidators(metadata, controllerContext)).Returns(new ModelValidator[]
            {
                allValidators[2], allValidators[3]
            });

            Resolver<IEnumerable<ModelValidatorProvider>> resolver = new Resolver<IEnumerable<ModelValidatorProvider>>();
            resolver.Current = new ModelValidatorProvider[] { resolverProvider1.Object, resolverprovider2.Object };

            ModelValidatorProviderCollection collection = new ModelValidatorProviderCollection(resolver);

            // Act
            IEnumerable<ModelValidator> returnedValidators = collection.GetValidators(metadata, controllerContext);

            // Assert
            Assert.Equal(allValidators, returnedValidators.ToArray());
        }

        private static ModelMetadata GetMetadata()
        {
            ModelMetadataProvider provider = new EmptyModelMetadataProvider();
            return provider.GetMetadataForType(null, typeof(object));
        }

        private sealed class SimpleModelValidator : ModelValidator
        {
            public SimpleModelValidator()
                : base(GetMetadata(), new ControllerContext())
            {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container)
            {
                throw new NotImplementedException();
            }
        }
    }
}
