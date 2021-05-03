// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Types;
#if NETCORE
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#else
using System.Net.Http;
using System.Web.Http;
#endif
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    [Collection("TimeZoneTests")] // TimeZoneInfo is not thread-safe. Tests in this collection will be executed sequentially 
    public class DeserializationHelpersTest
    {
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
                DeserializationHelpers.GetPropertyType(resource, propertyName));
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
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

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
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                String.Format("The property '{0}' on type 'Microsoft.AspNet.OData.Test.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithSettableCollectionProperties' returned a null value. " +
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
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

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
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                String.Format("The value of the property '{0}' on type 'Microsoft.AspNet.OData.Test.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' is an array. " +
                "Consider adding a setter for the property.", propertyName));
        }

        [Theory]
        [InlineData("CustomCollectionWithoutAdd")]
        public void SetCollectionProperty_NonSettableProperty_NonNullValue_NoAdd_Throws(string propertyName)
        {
            object value = new SampleClassWithNonSettableCollectionProperties();
            Type propertyType = typeof(SampleClassWithNonSettableCollectionProperties).GetProperty(propertyName).PropertyType;
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                String.Format("The type '{0}' of the property '{1}' on type 'Microsoft.AspNet.OData.Test.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' does not have an Add method. " +
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
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
                 String.Format("The property '{0}' on type 'Microsoft.AspNet.OData.Test.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithNonSettableCollectionProperties' returned a null value. " +
                 "The input stream contains collection items which cannot be added if the instance is null.", propertyName));
        }

        [Fact]
        public void SetCollectionProperty_CanConvertNonStandardEdmTypes()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("UnsignedArray", EdmPrimitiveTypeKind.Int32);

            DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name);

            Assert.Equal(
                new uint[] { 1, 2, 3 },
               value.UnsignedArray);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertDataTime_ByDefault()
        {
            // Arrange
            SampleClassWithDifferentCollectionProperties source = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("DateTimeList", EdmPrimitiveTypeKind.DateTimeOffset);
            TimeZoneInfoHelper.TimeZone = null;
            DateTime dt = new DateTime(2014, 11, 15, 1, 2, 3);
            IList<DateTimeOffset> dtos = new List<DateTimeOffset>
            {
                new DateTimeOffset(dt, TimeSpan.Zero),
                new DateTimeOffset(dt, new TimeSpan(+7, 0, 0)),
                new DateTimeOffset(dt, new TimeSpan(-8, 0, 0))
            };

            IEnumerable<DateTime> expects = dtos.Select(e => e.LocalDateTime);

            // Act
            DeserializationHelpers.SetCollectionProperty(source, edmProperty, dtos, edmProperty.Name);

            // Assert
            Assert.Equal(expects, source.DateTimeList);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertDataTime_ByTimeZoneInfo()
        {
            // Arrange
            SampleClassWithDifferentCollectionProperties source = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("DateTimeList", EdmPrimitiveTypeKind.DateTimeOffset);

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            TimeZoneInfoHelper.TimeZone = tzi;
            DateTime dt = new DateTime(2014, 11, 15, 1, 2, 3);
            IList<DateTimeOffset> dtos = new List<DateTimeOffset>
            {
                new DateTimeOffset(dt, TimeSpan.Zero),
                new DateTimeOffset(dt, new TimeSpan(+7, 0, 0)),
                new DateTimeOffset(dt, new TimeSpan(-8, 0, 0))
            };

            // Act
            DeserializationHelpers.SetCollectionProperty(source, edmProperty, dtos, edmProperty.Name);

            // Assert
            Assert.Equal(new List<DateTime> { dt.AddHours(-8), dt.AddHours(-15), dt }, source.DateTimeList);
        }

        [Fact]
        public void SetCollectionProperty_CanConvertEnumCollection()
        {
            SampleClassWithDifferentCollectionProperties value = new SampleClassWithDifferentCollectionProperties();
            IEdmProperty edmProperty = GetMockEdmProperty("FlagsEnum", EdmPrimitiveTypeKind.String);

            DeserializationHelpers.SetCollectionProperty(
                value,
                edmProperty,
                value: new List<FlagsEnum> { FlagsEnum.One, FlagsEnum.Four | FlagsEnum.Two | (FlagsEnum)123 },
                propertyName: edmProperty.Name);

            Assert.Equal(
                new FlagsEnum[] { FlagsEnum.One, FlagsEnum.Four | FlagsEnum.Two | (FlagsEnum)123 },
               value.FlagsEnum);
        }

        [Theory]
        [InlineData("NonCollectionString")]
        [InlineData("NonCollectionInt")]
        public void SetCollectionProperty_OnNonCollection_ThrowsSerialization(string propertyName)
        {
            object value = new SampleClassWithDifferentCollectionProperties();
            Type propertyType = typeof(SampleClassWithDifferentCollectionProperties).GetProperty(propertyName).PropertyType;
            IEdmProperty edmProperty = GetMockEdmProperty(propertyName, EdmPrimitiveTypeKind.Int32);

            ExceptionAssert.Throws<SerializationException>(
                () => DeserializationHelpers.SetCollectionProperty(value, edmProperty, value: new List<int> { 1, 2, 3 }, propertyName: edmProperty.Name),
            Error.Format(
            "The type '{0}' of the property '{1}' on type 'Microsoft.AspNet.OData.Test.Formatter.Deserialization.DeserializationHelpersTest+SampleClassWithDifferentCollectionProperties' must be a collection.",
            propertyType.FullName,
            propertyName));
        }

        [Theory]
        [InlineData("ICollection")]
        [InlineData("IList")]
        [InlineData("Collection")]
        [InlineData("List")]
        [InlineData("CustomCollectionWithNoEmptyCtor")]
        [InlineData("CustomCollection")]
        public void SetCollectionProperty_ClearsCollection_IfClearCollectionIsTrue(string propertyName)
        {
            // Arrange
            IEnumerable<int> value = new int[] { 1, 2, 3 };
            object resource = new SampleClassWithNonSettableCollectionProperties
                {
                    ICollection = { 42 },
                    IList = { 42 },
                    Collection = { 42 },
                    List = { 42 },
                    CustomCollectionWithNoEmptyCtor = { 42 },
                    CustomCollection = { 42 }
                };

            // Act
            DeserializationHelpers.SetCollectionProperty(resource, propertyName, null, value, clearCollection: true);

            // Assert
            Assert.Equal(
                value,
                resource.GetType().GetProperty(propertyName).GetValue(resource, index: null) as IEnumerable<int>);
        }

        [Fact]
        public void ApplyProperty_DoesNotIgnoreKeyProperty()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1",
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));

            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

            var resource = new Mock<IDelta>(MockBehavior.Strict);
            Type propertyType = typeof(string);
            resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true).Verifiable();
            resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

            // Act
            DeserializationHelpers.ApplyProperty(property, entityTypeReference, resource.Object, provider,
                new ODataDeserializerContext{ Model = new EdmModel() });

            // Assert
            resource.Verify();
        }

        [Fact]
        public void ApplyProperty_DoesNotIgnoreKeyProperty_WithInstanceAnnotation()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "Key1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1",
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));

            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

            var resource = new Mock<IDelta>(MockBehavior.Strict);
            Type propertyType = typeof(string);
            resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true).Verifiable();
            resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

            // Act
            DeserializationHelpers.ApplyInstanceAnnotations(resource.Object, entityTypeReference, null,provider,
    new ODataDeserializerContext { Model = new EdmModel() });

            DeserializationHelpers.ApplyProperty(property, entityTypeReference, resource.Object, provider,
                new ODataDeserializerContext { Model = new EdmModel() });

            // Assert
            resource.Verify();
        }

        [Fact]
        public void ApplyProperty_FailsWithUsefulErrorMessageOnUnknownProperty()
        {
            // Arrange
            const string HelpfulErrorMessage =
                "The property 'Unknown' does not exist on type 'namespace.name'. Make sure to only use property names " +
                "that are defined by the type.";

            var property = new ODataProperty { Name = "Unknown", Value = "Value" };
            var entityType = new EdmComplexType("namespace", "name");
            entityType.AddStructuralProperty("Known", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));

            var entityTypeReference = new EdmComplexTypeReference(entityType, isNullable: false);

            // Act
            var exception = Assert.Throws<ODataException>(() =>
                DeserializationHelpers.ApplyProperty(
                    property,
                    entityTypeReference,
                    resource: null,
                    deserializerProvider: null,
                    readContext: null));

            // Assert
            Assert.Equal(HelpfulErrorMessage, exception.Message);
        }


        [Fact]
        public void ApplyAnnotations_FailsWithUsefulErrorMessageOnUnknownProperty()
        {
            // Arrange
            const string HelpfulErrorMessage =
                "The property 'Unknown' does not exist on type 'namespace.name'. Make sure to only use property names " +
                "that are defined by the type.";

            var property = new ODataProperty { Name = "Unknown", Value = "Value" };
            var entityType = new EdmComplexType("namespace", "name");
            entityType.AddStructuralProperty("Known", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));

            var entityTypeReference = new EdmComplexTypeReference(entityType, isNullable: false);

            // Act
            var exception = Assert.Throws<ODataException>(() =>
                DeserializationHelpers.ApplyProperty(
                    property,
                    entityTypeReference,
                    resource: null,
                    deserializerProvider: null,
                    readContext: null));

            // Assert
            Assert.Equal(HelpfulErrorMessage, exception.Message);
        }
        
        [Fact]
        public void ApplyProperty_PassesWithCaseInsensitivePropertyName()
        {
            // Arrange
            ODataProperty property = new ODataProperty { Name = "keY1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddKeys(entityType.AddStructuralProperty("Key1",
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));

            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

            var resource = new Mock<IDelta>(MockBehavior.Strict);
            Type propertyType = typeof(string);
            resource.Setup(r => r.TryGetPropertyType("Key1", out propertyType)).Returns(true).Verifiable();
            resource.Setup(r => r.TrySetPropertyValue("Key1", "Value1")).Returns(true).Verifiable();

#if NETCORE

            IRouteBuilder builder = RoutingConfigurationFactory.Create();

            HttpRequest request = RequestFactory.Create(builder);
#else
            HttpConfiguration configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            HttpRequestMessage request = RequestFactory.Create(configuration);
#endif

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = new EdmModel(),
                Request = request
            };

            // Act
            DeserializationHelpers.ApplyProperty(property, entityTypeReference, resource.Object, provider,
                context);

            // Assert
            resource.Verify();
        }

        [Fact]
        public void ApplyProperty_FailWithTwoCaseInsensitiveMatchesAndCaseSensitiveMatch()
        {
            const string expectedErrorMessage = "The property 'proPerty1' does not exist on type 'namespace.name'. Make sure to only use property names that are defined by the type.";
            // Arrange
            ODataProperty property = new ODataProperty { Name = "proPerty1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddStructuralProperty("Property1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
            entityType.AddStructuralProperty("property1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
            entityType.AddKeys(entityType.AddStructuralProperty("Key1",
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));

            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

#if NETCORE

            IRouteBuilder builder = RoutingConfigurationFactory.Create();

            HttpRequest request = RequestFactory.Create(builder);
#else
            HttpConfiguration configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            HttpRequestMessage request = RequestFactory.Create(configuration);
#endif

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = new EdmModel(),
                Request = request
            };

            // Act
            var exception = Assert.Throws<ODataException>(() =>
                DeserializationHelpers.ApplyProperty(
                    property,
                    entityTypeReference,
                    resource: null,
                    provider,
                    context));

            // Assert
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        [Fact]
        public void ApplyProperty_FailWithCaseInsensitiveMatchesAndDisabledCaseSensitiveRequestPropertyBinding()
        {
            const string expectedErrorMessage = "The property 'proPerty1' does not exist on type 'namespace.name'. Make sure to only use property names that are defined by the type.";
            // Arrange
            ODataProperty property = new ODataProperty { Name = "proPerty1", Value = "Value1" };
            EdmEntityType entityType = new EdmEntityType("namespace", "name");
            entityType.AddStructuralProperty("Property1", EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string)));
            entityType.AddKeys(entityType.AddStructuralProperty("Key1",
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string))));

            EdmEntityTypeReference entityTypeReference = new EdmEntityTypeReference(entityType, isNullable: false);
            ODataDeserializerProvider provider = ODataDeserializerProviderFactory.Create();

#if NETCORE

            IRouteBuilder builder = RoutingConfigurationFactory.CreateWithDisabledCaseInsensitiveRequestPropertyBinding();

            HttpRequest request = RequestFactory.Create(builder);
#else
            HttpConfiguration configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            configuration.SetCompatibilityOptions(CompatibilityOptions.DisableCaseInsensitiveRequestPropertyBinding);
            HttpRequestMessage request = RequestFactory.Create(configuration);
#endif

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = new EdmModel(),
                Request = request
            };

            // Act
            var exception = Assert.Throws<ODataException>(() =>
                DeserializationHelpers.ApplyProperty(
                    property,
                    entityTypeReference,
                    resource: null,
                    provider,
                    context));

            // Assert
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        private static IEdmProperty GetMockEdmProperty(string name, EdmPrimitiveTypeKind elementType)
        {
            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns(name);
            IEdmTypeReference elementTypeReference =
                EdmCoreModel.Instance.GetPrimitiveType(elementType).ToEdmTypeReference(isNullable: false);
            property.Setup(p => p.Type)
                    .Returns(new EdmCollectionTypeReference(new EdmCollectionType(elementTypeReference)));
            return property.Object;
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

            public IList<DateTime> DateTimeList { get; set; }
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

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
