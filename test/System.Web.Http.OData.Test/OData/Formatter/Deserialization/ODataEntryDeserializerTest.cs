// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataEntryDeserializerTest
    {
        [Fact]
        public void Ctor_Throws_ArgumentNullForEdmType()
        {
            Assert.ThrowsArgumentNull(
                () =>
                {
                    var deserializer = new Mock<ODataEntryDeserializer>(null, ODataPayloadKind.Unsupported).Object;
                },
                "edmType");
        }

        [Fact]
        public void Ctor_SetsProperty_ODataPayloadKind()
        {
            var deserializer = new Mock<ODataEntryDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported);

            Assert.Equal(ODataPayloadKind.Unsupported, deserializer.Object.ODataPayloadKind);
        }

        [Fact]
        public void Ctor_SetsProperty_EdmType()
        {
            IEdmTypeReference edmType = new Mock<IEdmTypeReference>().Object;
            var deserializer = new Mock<ODataEntryDeserializer>(edmType, ODataPayloadKind.Unsupported);

            Assert.Same(edmType, deserializer.Object.EdmType);
        }

        [Fact]
        public void Ctor_SetsProperty_DeserializerProvider()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var deserializer = new Mock<ODataEntryDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported, deserializerProvider.Object);

            Assert.Same(deserializerProvider.Object, deserializer.Object.DeserializerProvider);
        }

        [Fact]
        public void ReadInline_Throws_NotSupported()
        {
            var deserializer = new Mock<ODataEntryDeserializer>(new Mock<IEdmTypeReference>().Object, ODataPayloadKind.Unsupported) { CallBase = true };

            Assert.Throws<NotSupportedException>(
                () => deserializer.Object.ReadInline(item: null, readContext: null),
                "Type 'ODataEntryDeserializerProxy' does not support ReadInline.");
        }

        [Theory]
        [InlineData("Property", true, typeof(int))]
        [InlineData("Property", false, typeof(int))]
        [InlineData("PropertyNotPresent", true, null)]
        [InlineData("PropertyNotPresent", false, null)]
        public void GetPropertyType_NonDelta(string propertyName, bool isDelta, Type expectedPropertyType)
        {
            object resource = isDelta ? (object)new Delta<GetPropertyType_TestClass>() : new GetPropertyType_TestClass();
            Assert.Equal(
                expectedPropertyType,
                ODataEntryDeserializer.GetPropertyType(resource, propertyName, isDelta));
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_CollectionTypeCanBeInstantiated_And_SettableProperty(string propertyName)
        {
            object value = new SampleClassWithSettableCollectionProperties();
            ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 });
            Assert.Equal(
                new[] { 1, 2, 3 },
                value.GetType().GetProperty(propertyName).GetValue(value, index: null) as IEnumerable<int>);
        }

        [Theory]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("ICustomCollectionInterface")]
        public void SetCollectionProperty_CollectionTypeCannotBeInstantiated_And_SettableProperty_Throws(string propertyName)
        {
            object value = new SampleClassWithSettableCollectionProperties();

            Assert.Throws<SerializationException>(
                () => ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 }),
                String.Format("The property '{0}' on type 'System.Web.Http.OData.Formatter.Deserialization.ODataEntryDeserializerTest+SampleClassWithSettableCollectionProperties' returned a null value. " +
                "The input stream contains collection items which cannot be added if the instance is null.", propertyName));
        }

        [Theory]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_NonSettableProperty_NonNullValue_WithAddMethod(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 });
            Assert.Equal(
                new[] { 1, 2, 3 },
                value.GetType().GetProperty(propertyName).GetValue(value, index: null) as IEnumerable<int>);
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        public void SetCollectionProperty_NonSettableProperty_ArrayValue_FixedSize_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            Type propertyType = typeof(SampleClassWithNonSettableCollectionProperties).GetProperty(propertyName).PropertyType;

            Assert.Throws<SerializationException>(
                () => ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 }),
                String.Format("The value of the property '{0}' on type 'System.Web.Http.OData.Formatter.Deserialization.ODataEntryDeserializerTest+SampleClassWithNonSettableCollectionProperties' is an array. " +
                "Consider adding a setter for the property.", propertyName));
        }

        [Theory]
        [InlineData("CustomCollectionWithoutAdd")]
        public void SetCollectionProperty_NonSettableProperty_NonNullValue_NoAdd_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            Type propertyType = typeof(SampleClassWithNonSettableCollectionProperties).GetProperty(propertyName).PropertyType;
            Assert.Throws<SerializationException>(
                () => ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 }),
                String.Format("The type '{0}' of the property '{1}' on type 'System.Web.Http.OData.Formatter.Deserialization.ODataEntryDeserializerTest+SampleClassWithNonSettableCollectionProperties' does not have an Add method. " +
                "Consider using a collection type that does have an Add method - for example IList<T> or ICollection<T>.", propertyType.FullName, propertyName));
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("IEnumerable")]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_NonSettableProperty_NullValue_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            value.GetType().GetProperty(propertyName).SetValue(value, null, null);

            Assert.Throws<SerializationException>(
                 () => ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 }),
                 String.Format("The property '{0}' on type 'System.Web.Http.OData.Formatter.Deserialization.ODataEntryDeserializerTest+SampleClassWithNonSettableCollectionProperties' returned a null value. " +
                 "The input stream contains collection items which cannot be added if the instance is null.", propertyName));
        }

        [Fact]
        public void SetCollectionProperty_CanConvertNonStandardEdmTypes()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            ODataEntityDeserializer.SetCollectionProperty(value, "UnsignedArray", isDelta: false, value: new List<int> { 1, 2, 3 });
            Assert.Equal(
                new uint[] { 1, 2, 3 },
               value.UnsignedArray);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertEnumCollection()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            ODataEntityDeserializer.SetCollectionProperty(value, "FlagsEnum", isDelta: false, value: new List<string> { "One", "Four, Two" });
            Assert.Equal(
                new FlagsEnum[] { FlagsEnum.One, FlagsEnum.Four | FlagsEnum.Two },
               value.FlagsEnum);
        }

        [Theory]
        [InlineData("NonCollectionString")]
        [InlineData("NonCollectionInt")]
        public void SetCollectionProperty_OnNonCollection_ThrowsSerialization(string propertyName)
        {
            object value = new SampleClassWithDifferentCollectionProperties();
            Type propertyType = typeof(SampleClassWithDifferentCollectionProperties).GetProperty(propertyName).PropertyType;

            Assert.Throws<SerializationException>(
            () => ODataEntityDeserializer.SetCollectionProperty(value, propertyName, isDelta: false, value: new List<int> { 1, 2, 3 }),
            Error.Format(
            "The type '{0}' of the property '{1}' on type 'System.Web.Http.OData.Formatter.Deserialization.ODataEntryDeserializerTest+SampleClassWithDifferentCollectionProperties' must be a collection.",
            propertyType.FullName,
            propertyName));
        }

        [Fact]
        public void ApplyProperty_DoesNotIgnoreKeyProperty()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(new EdmStructuralProperty(entityType, "Key1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));
            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = new DefaultODataDeserializerProvider();

            var resource = new Mock<IDelta>(MockBehavior.Strict);
            Type propertyType = typeof(string);
            resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true).Verifiable();
            resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

            // Act
            ODataEntryDeserializer.ApplyProperty(property, entityTypeReference, resource.Object, provider, new ODataDeserializerContext { IsPatchMode = true });

            // Assert
            resource.Verify();
        }

        private class GetPropertyType_TestClass
        {
            public int Property { get; set; }
        }

        private class SampleClassWithSettableCollectionProperties
        {
            public int[] Array { get; set; }

            public IEnumerable<int> IEnumerable { get; set; }

            public ICollection<int> ICollection { get; set; }

            public IList<int> IList { get; set; }

            public Collection<int> Collection { get; set; }

            public List<int> List { get; set; }

            public CustomCollection CustomCollection { get; set; }

            public CustomCollectionWithNoEmptyCtor CustomCollectionWithNoEmptyCtor { get; set; }

            public ICustomCollectionInterface<int> ICustomCollectionInterface { get; set; }
        }

        private class SampleClassWithNonSettableCollectionProperties
        {
            public SampleClassWithNonSettableCollectionProperties()
            {
                Array = new int[0];
                IEnumerable = new int[0];
                ICollection = new Collection<int>();
                IList = new List<int>();
                Collection = new Collection<int>();
                List = new List<int>();
                CustomCollection = new CustomCollection();
                CustomCollectionWithNoEmptyCtor = new CustomCollectionWithNoEmptyCtor(100);
                CustomCollectionWithoutAdd = new CustomCollectionWithoutAdd<int>();
            }

            public int[] Array { get; internal set; }

            public IEnumerable<int> IEnumerable { get; internal set; }

            public ICollection<int> ICollection { get; internal set; }

            public IList<int> IList { get; internal set; }

            public Collection<int> Collection { get; internal set; }

            public List<int> List { get; internal set; }

            public CustomCollection CustomCollection { get; internal set; }

            public CustomCollectionWithNoEmptyCtor CustomCollectionWithNoEmptyCtor { get; internal set; }

            public CustomCollectionWithoutAdd<int> CustomCollectionWithoutAdd { get; internal set; }
        }

        private class SampleClassWithDifferentCollectionProperties
        {
            public string NonCollectionString { get; set; }

            public int NonCollectionInt { get; set; }

            public uint[] UnsignedArray { get; set; }

            public FlagsEnum[] FlagsEnum { get; set; }
        }

        private class CustomCollection : List<int> { }

        private class CustomCollectionWithNoEmptyCtor : List<int>
        {
            public CustomCollectionWithNoEmptyCtor(int i)
            {
            }
        }

        private interface ICustomCollectionInterface<T> : IEnumerable<T>
        {
        }

        private class CustomCollectionWithoutAdd<T> : IEnumerable<T>
        {
            private List<T> _list = new List<T>();

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
