// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing
{
    public class SingletonPathSegmentTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_IfMissSingleton()
        {
            Assert.ThrowsArgumentNull(() => new SingletonPathSegment(singleton: null), "singleton");
        }

        [Fact]
        public void Ctor_ThrowsArgumentException_IfMissSingletonName()
        {
            Assert.Throws<ArgumentException>(() => new SingletonPathSegment(singletonName: null));
        }

        [Fact]
        public void Ctor_TakingSingleton_ToInitializesSingletonProperty()
        {
            // Arrange
            Mock<IEdmSingleton> edmSingleton= new Mock<IEdmSingleton>();
            edmSingleton.Setup(a => a.Name).Returns("Vip");

            // Act
            SingletonPathSegment singletonPathSegment = new SingletonPathSegment(edmSingleton.Object);

            // Assert
            Assert.Same(edmSingleton.Object, singletonPathSegment.Singleton);
        }

        [Fact]
        public void Ctor_TakingSingleton_ToInitializesSingletonNameProperty()
        {
            // Arrange
            Mock<IEdmSingleton> edmSingleton = new Mock<IEdmSingleton>();
            edmSingleton.Setup(a => a.Name).Returns("Vip");

            // Act
            SingletonPathSegment singletonPathSegment = new SingletonPathSegment(edmSingleton.Object);

            // Assert
            Assert.Equal("Vip", singletonPathSegment.SingletonName);
        }

        [Fact]
        public void Ctor_TakingSingletonName_ToInitializesSingletonNameProperty()
        {
            // Arrange
            SingletonPathSegment singletonPathSegment = new SingletonPathSegment("Vip");

            // Act & Assert
            Assert.Equal("Vip", singletonPathSegment.SingletonName);
        }

        [Fact]
        public void Property_SegmentKind_ReturnsSingleton()
        {
            // Arrange
            SingletonPathSegment segment = new SingletonPathSegment("Vip");

            // Act & Assert
            Assert.Equal(ODataSegmentKinds.Singleton, segment.SegmentKind);
        }

        [Fact]
        public void GetEdmType_Returns_SingletonEntityType()
        {
            // Arrange
            Mock<IEdmEntityContainer> edmContainer = new Mock<IEdmEntityContainer>();
            Mock<IEdmEntityType> entityType = new Mock<IEdmEntityType>();
            IEdmSingleton edmSingleton = new EdmSingleton(edmContainer.Object, "singleton", entityType.Object);

            // Act
            SingletonPathSegment segment = new SingletonPathSegment(edmSingleton);

            // Assert
            Assert.Same(entityType.Object, segment.GetEdmType(previousEdmType: null));
        }

        [Fact]
        public void GetNavigationSource_Returns_SingletonTargetNavigationSource()
        {
            // Arrange
            Mock<IEdmSingleton> edmSingleton = new Mock<IEdmSingleton>();
            edmSingleton.Setup(a => a.Name).Returns("Vip");

            // Act
            SingletonPathSegment segment = new SingletonPathSegment(edmSingleton.Object);

            // Assert
            Assert.Same(edmSingleton.Object, segment.GetNavigationSource(previousNavigationSource: null));
        }

        [Fact]
        public void ToString_Returns_SingletonName()
        {
            // Arrange
            SingletonPathSegment segment = new SingletonPathSegment(singletonName: "Vip");

            // Act & Assert
            Assert.Equal("Vip", segment.ToString());
        }

        [Fact]
        public void TryMatch_ReturnsTrue_IfThePathSegmentRefersToSameSingleton()
        {
            // Arrange
            Mock<IEdmSingleton> edmSingleton = new Mock<IEdmSingleton>();
            edmSingleton.Setup(a => a.Name).Returns("Vip");

            SingletonPathSegment pathSegmentTemplate = new SingletonPathSegment(edmSingleton.Object);
            SingletonPathSegment pathSegment = new SingletonPathSegment(edmSingleton.Object);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act & Assert
            Assert.True(pathSegmentTemplate.TryMatch(pathSegment, values));
            Assert.Empty(values);
        }

        [Fact]
        public void TryMatch_ReturnsFalse_IfThePathSegmentRefersToDifferentSingleton()
        {
            // Arrange
            Mock<IEdmSingleton> edmSingleton = new Mock<IEdmSingleton>();
            edmSingleton.Setup(a => a.Name).Returns("Vip");

            Mock<IEdmSingleton> templateSingleton = new Mock<IEdmSingleton>();
            templateSingleton.Setup(a => a.Name).Returns("Vip");

            SingletonPathSegment pathSegmentTemplate = new SingletonPathSegment(templateSingleton.Object);
            SingletonPathSegment pathSegment = new SingletonPathSegment(edmSingleton.Object);
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Act & Assert
            Assert.False(pathSegmentTemplate.TryMatch(pathSegment, values));
        }
    }
}
