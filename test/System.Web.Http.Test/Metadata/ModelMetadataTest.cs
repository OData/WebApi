// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Metadata.Providers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Metadata
{
    public class ModelMetadataTest
    {
        // Guard clauses

        [Fact]
        public void NullProviderThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(null /* provider */, null /* containerType */, null /* model */, typeof(object), null /* propertyName */),
                "provider");
        }

        [Fact]
        public void NullTypeThrows()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => new ModelMetadata(provider.Object, null /* containerType */, null /* model */, null /* modelType */, null /* propertyName */),
                "modelType");
        }

        // Constructor

        [Fact]
        public void DefaultValues()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(Exception), () => "model", typeof(string), "propertyName");

            // Assert
            Assert.Equal(typeof(Exception), metadata.ContainerType);
            Assert.True(metadata.ConvertEmptyStringToNull);
            Assert.Null(metadata.DataTypeName);
            Assert.Null(metadata.Description);
            Assert.Null(metadata.DisplayFormatString);
            Assert.Null(metadata.DisplayName);
            Assert.Null(metadata.EditFormatString);
            Assert.False(metadata.HideSurroundingHtml);
            Assert.Equal("model", metadata.Model);
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Null(metadata.NullDisplayText);
            Assert.Equal(10000, metadata.Order);
            Assert.Equal("propertyName", metadata.PropertyName);
            Assert.False(metadata.IsReadOnly);
            Assert.True(metadata.RequestValidationEnabled);
            Assert.Null(metadata.ShortDisplayName);
            Assert.True(metadata.ShowForDisplay);
            Assert.True(metadata.ShowForEdit);
            Assert.Null(metadata.TemplateHint);
            Assert.Null(metadata.Watermark);
        }

        // IsComplexType

        struct IsComplexTypeModel
        {
        }

        [Fact]
        public void IsComplexTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Object), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsComplexType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(IsComplexTypeModel), null).IsComplexType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<IsComplexTypeModel>), null).IsComplexType);
        }

        // IsNullableValueType

        [Fact]
        public void IsNullableValueTypeTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsNullableValueType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsNullableValueType);
        }

        // IsRequired

        [Fact]
        public void IsRequiredTests()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsRequired); // Reference type not required
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsRequired); // Interface not required
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsRequired); // Nullable value type not required
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsRequired); // Value type required
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(DayOfWeek), null).IsRequired); // Enum (implicit value type) is required
        }

        // Properties

        [Fact]
        public void PropertiesCallsProvider()
        {
            // Arrange
            Type modelType = typeof(string);
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>();
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.Equal(propertyMetadata, result.ToList());
            provider.Verify();
        }

        [Fact]
        public void PropertiesAreSortedByOrder()
        {
            // Arrange
            Type modelType = typeof(string);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>
            {
                new ModelMetadata(provider.Object, null, () => 1, typeof(int), null) { Order = 20 },
                new ModelMetadata(provider.Object, null, () => 2, typeof(int), null) { Order = 30 },
                new ModelMetadata(provider.Object, null, () => 3, typeof(int), null) { Order = 10 },
            };
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            List<ModelMetadata> result = metadata.Properties.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0].Model);
            Assert.Equal(1, result[1].Model);
            Assert.Equal(2, result[2].Model);
        }

        [Fact]
        public void PropertiesListGetsResetWhenModelGetsReset()
        { // Dev10 Bug #923263
            // Arrange
            var provider = new CachedDataAnnotationsModelMetadataProvider();
            var metadata = new ModelMetadata(provider, null, () => new Class1(), typeof(Class1), null);

            // Act
            ModelMetadata[] originalProps = metadata.Properties.ToArray();
            metadata.Model = new Class2();
            ModelMetadata[] newProps = metadata.Properties.ToArray();

            // Assert
            ModelMetadata originalProp = Assert.Single(originalProps);
            Assert.Equal(typeof(string), originalProp.ModelType);
            Assert.Equal("Prop1", originalProp.PropertyName);
            ModelMetadata newProp = Assert.Single(newProps);
            Assert.Equal(typeof(int), newProp.ModelType);
            Assert.Equal("Prop2", newProp.PropertyName);
        }

        class Class1
        {
            public string Prop1 { get; set; }
        }

        class Class2
        {
            public int Prop2 { get; set; }
        }

        // SimpleDisplayText

        [Fact]
        public void SimpleDisplayTextReturnsNullDisplayTextForNullModel()
        {
            // Arrange
            string nullText = "(null)";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null) { NullDisplayText = nullText };

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(nullText, result);
        }

        private class SimpleDisplayTextModelWithToString
        {
            public override string ToString()
            {
                return "Custom ToString Value";
            }
        }

        [Fact]
        public void SimpleDisplayTextReturnsToStringValueWhenOverridden()
        {
            // Arrange
            SimpleDisplayTextModelWithToString model = new SimpleDisplayTextModelWithToString();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(model.ToString(), result);
        }

        private class SimpleDisplayTextModelWithoutToString
        {
            public string FirstProperty { get; set; }

            public int SecondProperty { get; set; }
        }

        [Fact]
        public void SimpleDisplayTextReturnsFirstPropertyValueForNonNullModel()
        {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString
            {
                FirstProperty = "First Property Value"
            };
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(model.FirstProperty, result);
        }

        [Fact]
        public void SimpleDisplayTextReturnsFirstPropertyNullDisplayTextForNonNullModelWithNullDisplayColumnPropertyValue()
        {
            // Arrange
            SimpleDisplayTextModelWithoutToString model = new SimpleDisplayTextModelWithoutToString();
            EmptyModelMetadataProvider propertyProvider = new EmptyModelMetadataProvider();
            ModelMetadata propertyMetadata = propertyProvider.GetMetadataForProperty(() => model.FirstProperty, typeof(SimpleDisplayTextModelWithoutToString), "FirstProperty");
            propertyMetadata.NullDisplayText = "Null Display Text";
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            provider.Setup(p => p.GetMetadataForProperties(model, typeof(SimpleDisplayTextModelWithoutToString)))
                .Returns(new[] { propertyMetadata });
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(SimpleDisplayTextModelWithoutToString), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(propertyMetadata.NullDisplayText, result);
        }

        private class SimpleDisplayTextModelWithNoProperties
        {
        }

        [Fact]
        public void SimpleDisplayTextReturnsEmptyStringForNonNullModelWithNoVisibleProperties()
        {
            // Arrange
            SimpleDisplayTextModelWithNoProperties model = new SimpleDisplayTextModelWithNoProperties();
            EmptyModelMetadataProvider provider = new EmptyModelMetadataProvider();
            ModelMetadata metadata = new ModelMetadata(provider, null, () => model, typeof(SimpleDisplayTextModelWithNoProperties), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(String.Empty, result);
        }

        private class ObjectWithToStringOverride
        {
            private string _toStringValue;

            public ObjectWithToStringOverride(string toStringValue)
            {
                _toStringValue = toStringValue;
            }

            public override string ToString()
            {
                return _toStringValue;
            }
        }

        [Fact]
        public void SimpleDisplayTextReturnsToStringOfModelForNonNullModel()
        {
            // Arrange
            string toStringText = "text from ToString()";
            ObjectWithToStringOverride model = new ObjectWithToStringOverride(toStringText);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(ObjectWithToStringOverride), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(toStringText, result);
        }

        [Fact]
        public void SimpleDisplayTextReturnsEmptyStringForNonNullModelWithToStringNull()
        {
            // Arrange
            string toStringText = null;
            ObjectWithToStringOverride model = new ObjectWithToStringOverride(toStringText);
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, () => model, typeof(ObjectWithToStringOverride), null);

            // Act
            string result = metadata.SimpleDisplayText;

            // Assert
            Assert.Equal(String.Empty, result);
        }

        // GetDisplayName()

        [Fact]
        public void ReturnsDisplayNameWhenSet()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName") { DisplayName = "Display Name" };

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Display Name", result);
        }

        [Fact]
        public void ReturnsPropertyNameWhenSetAndDisplayNameIsNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName");

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("PropertyName", result);
        }

        [Fact]
        public void ReturnsTypeNameWhenPropertyNameAndDisplayNameAreNull()
        {
            // Arrange
            Mock<ModelMetadataProvider> provider = new Mock<ModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Object", result);
        }

        // Helpers

        private class DummyContactModel
        {
            public int IntField = 0;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> NullableIntValue { get; set; }
            public int[] Array { get; set; }

            public string this[int index]
            {
                get { return "Indexed into " + index; }
            }
        }

        private class DummyModelContainer
        {
            public DummyContactModel Model { get; set; }
        }
    }
}
