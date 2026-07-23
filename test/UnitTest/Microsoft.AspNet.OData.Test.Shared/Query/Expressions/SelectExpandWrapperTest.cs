//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query.Expressions
{
    public class SelectExpandWrapperTest
    {
        private CustomersModelWithInheritance _model;
        private string _modelID;

        public SelectExpandWrapperTest()
        {
            _model = new CustomersModelWithInheritance();
            _modelID = ModelContainer.GetModelID(_model.Model);
        }

        [Fact]
        public void Property_Instance_RoundTrips()
        {
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>();
            ReflectionAssert.Property(wrapper, w => w.Instance, expectedDefaultValue: null, allowNull: true, roundTripTestValue: new TestEntity());
        }

        [Fact]
        public void Property_Container_RoundTrips()
        {
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>();

            ReflectionAssert.Property(
                wrapper, w => w.Container, expectedDefaultValue: null, allowNull: true, roundTripTestValue: new MockPropertyContainer());
        }

        [Fact]
        public void GetEdmType_Returns_InstanceType()
        {
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            _model.Model.SetAnnotationValue(_model.SpecialCustomer, new ClrTypeAnnotation(typeof(DerivedEntity)));
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity> { ModelID = _modelID };
            wrapper.Instance = new DerivedEntity();

            IEdmTypeReference edmType = wrapper.GetEdmType();

            Assert.Same(_model.SpecialCustomer, edmType.Definition);
        }

        [Fact]
        public void GetEdmType_Returns_ElementTypeIfInstanceIsNull()
        {
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
            _model.Model.SetAnnotationValue(_model.SpecialCustomer, new ClrTypeAnnotation(typeof(DerivedEntity)));
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity> { ModelID = _modelID };

            IEdmTypeReference edmType = wrapper.GetEdmType();

            Assert.Same(_model.Customer, edmType.Definition);
        }

        [Fact]
        public void TryGetValue_ReturnsValueFromPropertyContainer_IfPresent()
        {
            object expectedPropertyValue = new object();
            MockPropertyContainer container = new MockPropertyContainer();
            container.Properties.Add("SampleProperty", expectedPropertyValue);
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>
                {
                    ModelID = _modelID,
                    Container = container
                };
            wrapper.Instance = new TestEntity();

            object value;
            bool result = wrapper.TryGetPropertyValue("SampleProperty", out value);

            Assert.True(result);
            Assert.Same(expectedPropertyValue, value);
        }

        [Fact]
        public void TryGetValue_ReturnsValueFromInstance_IfNotPresentInContainer()
        {
            object expectedPropertyValue = new object();
            MockPropertyContainer container = new MockPropertyContainer();
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>
                {
                    ModelID = _modelID,
                    Container = container
                };
            wrapper.Instance = new TestEntity { SampleProperty = expectedPropertyValue };
            wrapper.UseInstanceForProperties = true;

            object value;
            bool result = wrapper.TryGetPropertyValue("SampleProperty", out value);

            Assert.True(result);
            Assert.Same(expectedPropertyValue, value);
        }

        [Fact]
        public void TryGetValue_ReturnsValueFromInstance_IfContainerIsNull()
        {
            object expectedPropertyValue = new object();
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity> { ModelID = _modelID };
            wrapper.Instance = new TestEntity { SampleProperty = expectedPropertyValue };
            wrapper.UseInstanceForProperties = true;

            object value;
            bool result = wrapper.TryGetPropertyValue("SampleProperty", out value);

            Assert.True(result);
            Assert.Same(expectedPropertyValue, value);
        }

        [Fact]
        public void TryGetValue_PropertyAliased_IfAnnotationSet()
        {
            // Arrange
            _model.Model.SetAnnotationValue(_model.Customer, new ClrTypeAnnotation(typeof(TestEntityWithAlias)));
            _model.Model.SetAnnotationValue(
                _model.CustomerName,
                new ClrPropertyInfoAnnotation(typeof(TestEntityWithAlias).GetProperty("SampleProperty")));
            object expectedPropertyValue = new object();
            SelectExpandWrapper<TestEntityWithAlias> wrapper = new SelectExpandWrapper<TestEntityWithAlias> { ModelID = _modelID };
            wrapper.Instance = new TestEntityWithAlias { SampleProperty = expectedPropertyValue };
            wrapper.UseInstanceForProperties = true;

            // Act
            object value;
            bool result = wrapper.TryGetPropertyValue("Name", out value);

            // Assert
            Assert.True(result);
            Assert.Same(expectedPropertyValue, value);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfContainerAndInstanceAreNull()
        {
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity> { ModelID = _modelID };

            object value;
            bool result = wrapper.TryGetPropertyValue("SampleProperty", out value);

            Assert.False(result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_IfPropertyNotPresentInElement()
        {
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity> { ModelID = _modelID };

            object value;
            bool result = wrapper.TryGetPropertyValue("SampleNotPresentProperty", out value);

            Assert.False(result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_ForClrPropertyNotDeclaredInEdmModel()
        {
            // Arrange: an open entity type that declares "Id" and "Name" but intentionally omits
            // "PasswordHash", even though the backing CLR class has it.
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "AccountEntity", null, false, isOpen: true);
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(AccountEntity)));

            SelectExpandWrapper<AccountEntity> wrapper = new SelectExpandWrapper<AccountEntity>
            {
                ModelID = ModelContainer.GetModelID(model),
                InstanceType = "NS.AccountEntity",
                Instance = new AccountEntity { Id = 1, Name = "Alice", PasswordHash = "clr-only-value" },
                UseInstanceForProperties = true,
            };

            // Act: request the CLR-only property that is not declared in the EDM model.
            object value;
            bool result = wrapper.TryGetPropertyValue("PasswordHash", out value);

            // Assert: the CLR fallback must not resolve the undeclared property.
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_ForClrPropertyDeclaredInEdmModel_OnOpenType()
        {
            // Arrange: same open entity type as above, but requesting the declared "Name" property
            // must continue to work via the CLR instance fallback.
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "AccountEntity", null, false, isOpen: true);
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(AccountEntity)));

            SelectExpandWrapper<AccountEntity> wrapper = new SelectExpandWrapper<AccountEntity>
            {
                ModelID = ModelContainer.GetModelID(model),
                InstanceType = "NS.AccountEntity",
                Instance = new AccountEntity { Id = 1, Name = "Alice", PasswordHash = "clr-only-value" },
                UseInstanceForProperties = true,
            };

            // Act
            object value;
            bool result = wrapper.TryGetPropertyValue("Name", out value);

            // Assert
            Assert.True(result);
            Assert.Equal("Alice", value);
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_ForDynamicPropertyDictionary_OnOpenType()
        {
            // Arrange: an open entity type whose dynamic-property container ("DynamicProperties")
            // is annotated on the model. Undeclared property names stored inside this bag must
            // still be reachable through the container/dictionary lookup, not through CLR reflection
            // of the "PasswordHash" property.
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "AccountEntity", null, false, isOpen: true);
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(AccountEntity)));
            model.SetAnnotationValue(
                entityType,
                new DynamicPropertyDictionaryAnnotation(typeof(AccountEntity).GetProperty("DynamicProperties")));

            SelectExpandWrapper<AccountEntity> wrapper = new SelectExpandWrapper<AccountEntity>
            {
                ModelID = ModelContainer.GetModelID(model),
                InstanceType = "NS.AccountEntity",
                Instance = new AccountEntity { Id = 1, Name = "Alice", PasswordHash = "clr-only-value" },
                UseInstanceForProperties = true,
            };

            // Act: request the container property itself (not an arbitrary undeclared name).
            object value;
            bool result = wrapper.TryGetPropertyValue("DynamicProperties", out value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TryGetValue_ReturnsFalse_ForNavigationPropertyNotInContainer_OnOpenType()
        {
            // Arrange: an open entity type that declares "Manager" as a *navigation* property.
            // Navigation properties are not structural, so a declared navigation property that is
            // absent from the container must not reach the CLR reflection fallback.
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "AccountEntity", null, false, isOpen: true);
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            entityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Manager",
                TargetMultiplicity = EdmMultiplicity.ZeroOrOne,
                Target = entityType,
            });
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(AccountEntity)));

            SelectExpandWrapper<AccountEntity> wrapper = new SelectExpandWrapper<AccountEntity>
            {
                ModelID = ModelContainer.GetModelID(model),
                InstanceType = "NS.AccountEntity",
                Instance = new AccountEntity { Id = 1, Name = "Alice", Manager = new AccountEntity { Id = 2, Name = "Bob" } },
                UseInstanceForProperties = true,
            };

            // Act: request the declared navigation property that is not present in the container.
            object value;
            bool result = wrapper.TryGetPropertyValue("Manager", out value);

            // Assert: the CLR fallback must not resolve the navigation property.
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void ToDictionary_ContainsAllStructuralProperties_IfInstanceIsNotNull()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(TestEntity)));
            entityType.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Int32);
            IEdmTypeReference edmType = new EdmEntityTypeReference(entityType, isNullable: false);
            SelectExpandWrapper<TestEntity> testWrapper = new SelectExpandWrapper<TestEntity>
            {
                Instance = new TestEntity { SampleProperty = 42 },
                ModelID = ModelContainer.GetModelID(model),
                UseInstanceForProperties = true,
            };

            // Act
            var result = testWrapper.ToDictionary();

            // Assert
            Assert.Equal(42, result["SampleProperty"]);
        }

        [Fact]
        public void ToDictionary_ContainsAllProperties_FromContainer()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(TestEntity)));
            entityType.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Int32);
            MockPropertyContainer container = new MockPropertyContainer();
            container.Properties.Add("Property", 42);
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>
            {
                Container = container,
                ModelID = ModelContainer.GetModelID(model)
            };

            // Act
            var result = wrapper.ToDictionary();

            // Assert
            Assert.Equal(42, result["Property"]);
        }

        [Fact]
        public void ToDictionary_Throws_IfMapperProviderIsNull()
        {
            // Arrange
            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentNullException>(() => wrapper.ToDictionary(mapperProvider: null));
        }

        [Fact]
        public void ToDictionary_Throws_IfMapperProvider_ReturnsNullPropertyMapper()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Int32);

            EdmModel model = new EdmModel();
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(TestEntity)));
            IEdmTypeReference edmType = new EdmEntityTypeReference(entityType, isNullable: false);

            SelectExpandWrapper<TestEntity> wrapper = new SelectExpandWrapper<TestEntity>
            {
                Instance = new TestEntity { SampleProperty = 42 },
                ModelID = ModelContainer.GetModelID(model)
            };

            Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider =
                (IEdmModel m, IEdmStructuredType t) => null;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => 
                wrapper.ToDictionary(mapperProvider: mapperProvider),
                "The mapper provider must return a valid 'Microsoft.AspNet.OData.Query.IPropertyMapper' instance for the given 'NS.Name' IEdmType.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ToDictionary_Throws_IfMappingIsNullOrEmpty_ForAGivenProperty(string propertyMapping)
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Int32);
            
            EdmModel model = new EdmModel();
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(TestEntity)));
            IEdmTypeReference edmType = new EdmEntityTypeReference(entityType, isNullable: false);
            
            SelectExpandWrapper<TestEntity> testWrapper = new SelectExpandWrapper<TestEntity>
            {
                Instance = new TestEntity { SampleProperty = 42 },
                ModelID = ModelContainer.GetModelID(model),
                UseInstanceForProperties = true,
            };

            Mock<IPropertyMapper> mapperMock = new Mock<IPropertyMapper>();
            mapperMock.Setup(m => m.MapProperty("SampleProperty")).Returns(propertyMapping);
            Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider =
                (IEdmModel m, IEdmStructuredType t) => mapperMock.Object;

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() =>
                testWrapper.ToDictionary(mapperProvider), 
                "The key mapping for the property 'SampleProperty' can't be null or empty.");
        }

        [Fact]
        public void ToDictionary_AppliesMappingToAllProperties_IfInstanceIsNotNull()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Name");
            entityType.AddStructuralProperty("SampleProperty", EdmPrimitiveTypeKind.Int32);
            
            EdmModel model = new EdmModel();
            model.AddElement(entityType);
            model.SetAnnotationValue(entityType, new ClrTypeAnnotation(typeof(TestEntity)));
            IEdmTypeReference edmType = new EdmEntityTypeReference(entityType, isNullable: false);
            
            SelectExpandWrapper<TestEntity> testWrapper = new SelectExpandWrapper<TestEntity>
            {
                Instance = new TestEntity { SampleProperty = 42 },
                ModelID = ModelContainer.GetModelID(model),
                UseInstanceForProperties = true,
            };
            
            Mock<IPropertyMapper> mapperMock = new Mock<IPropertyMapper>();
            mapperMock.Setup(m => m.MapProperty("SampleProperty")).Returns("Sample");
            Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider =
                (IEdmModel m, IEdmStructuredType t) => mapperMock.Object;

            // Act
            var result = testWrapper.ToDictionary(mapperProvider);

            // Assert
            Assert.Equal(42, result["Sample"]);
        }

        private class MockPropertyContainer : PropertyContainer
        {
            public MockPropertyContainer()
            {
                Properties = new Dictionary<string, object>();
            }

            public Dictionary<string, object> Properties { get; private set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                foreach (var kvp in Properties)
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private class TestEntity
        {
            public object SampleProperty { get; set; }
        }

        private class DerivedEntity : TestEntity
        {
        }

        // CLR class backing an open EDM entity type used to verify that CLR properties
        // excluded from the EDM model (e.g. via [NotMapped] or .Ignore()) are not resolved
        // through the SelectExpandWrapper CLR reflection fallback.
        private class AccountEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IDictionary<string, object> DynamicProperties { get; set; }

            // Declared as a navigation property in the EDM model (not a structural property).
            public AccountEntity Manager { get; set; }

            // Intentionally omitted from the EDM model in the tests above.
            public string PasswordHash { get; set; }
        }

        [DataContract(Namespace = "NS", Name = "Customer")]
        private class TestEntityWithAlias
        {
            [DataMember(Name = "Name")]
            public object SampleProperty { get; set; }
        }
    }
}
