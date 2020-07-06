// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class ComplexTypeTest
    {
        [Fact]
        [Trait("Description", "ODataModelBuilder can build ComplexType by convention")]
        public void CreateComplexTypeByConvention()
        {
            var builder = new ODataModelBuilder().Add_Address_ComplexType();

            var model = builder.GetServiceModel();
            var addressType = model.SchemaElements.OfType<IEdmComplexType>().Single();
            Assert.Equal("Address", addressType.Name);
            Assert.Equal(4, addressType.DeclaredProperties.Count());
            var houseNumberProperty = addressType.DeclaredProperties.SingleOrDefault(p => p.Name == "HouseNumber");
            var streetProperty = addressType.DeclaredProperties.SingleOrDefault(p => p.Name == "Street");
            var cityProperty = addressType.DeclaredProperties.SingleOrDefault(p => p.Name == "City");
            var stateProperty = addressType.DeclaredProperties.SingleOrDefault(p => p.Name == "State");
            var zipCodeProperty = addressType.DeclaredProperties.SingleOrDefault(p => p.Name == "ZipCode");

            Assert.NotNull(houseNumberProperty);
            Assert.NotNull(streetProperty);
            Assert.NotNull(cityProperty);
            Assert.NotNull(stateProperty);
            Assert.Null(zipCodeProperty);
            Assert.Equal("Edm.Int32", houseNumberProperty.Type.FullName());
            Assert.Equal("Edm.String", streetProperty.Type.FullName());
        }

        [Fact]
        [Trait("Description", "ODataModelBuilder can create a complex type property")]
        public void CreateComplexTypeProperty()
        {
            var builder = new ODataModelBuilder().Add_Customer_EntityType().Add_Address_ComplexType();
            builder.EntityType<Customer>().ComplexProperty(c => c.Address);
            var model = builder.GetServiceModel();
            var customerType = model.SchemaElements.OfType<IEdmEntityType>().Single();
            var addressProperty = customerType.FindProperty("Address");
            Assert.NotNull(addressProperty);
        }

        [Fact]
        [Trait("Description", "ODataModelBuilder can create a complex type property in a complex type")]
        public void CreateComplexTypePropertyInComplexType()
        {
            var builder = new ODataModelBuilder()
                .Add_Address_ComplexType();

            var address = builder.ComplexType<Address>();
            address.ComplexProperty(a => a.ZipCode);

            var model = builder.GetServiceModel();
            var addressType = model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(se => se.Name == "Address");
            var zipCodeType = model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(se => se.Name == "ZipCode");
            Assert.NotNull(addressType);
            Assert.NotNull(zipCodeType);
            var zipCodeProperty = addressType.FindProperty("ZipCode");
            Assert.NotNull(zipCodeProperty);
            Assert.Equal(zipCodeType.FullName(), zipCodeProperty.Type.AsComplex().ComplexDefinition().FullName());
        }

        [Fact]
        [Trait("Description", "ODataModelBuilder can create nested complex type properties (that infinitely recurse)")]
        public void CreateInfiniteRecursiveComplexTypeDefinitionSucceeds()
        {
            var builder = new ODataModelBuilder().Add_RecursiveZipCode_ComplexType();

            var zipCode = builder.ComplexType<RecursiveZipCode>();
            zipCode.ComplexProperty(z => z.Recursive);

            var model = builder.GetServiceModel();
            var recursiveZipCodeType = model.SchemaElements.OfType<IEdmComplexType>().SingleOrDefault(se => se.Name == "RecursiveZipCode");
            Assert.NotNull(recursiveZipCodeType);
            var recursiveZipCodeProperty = recursiveZipCodeType.FindProperty("Recursive");
            Assert.NotNull(recursiveZipCodeProperty);
            Assert.Equal(recursiveZipCodeType.FullName(), recursiveZipCodeProperty.Type.AsComplex().ComplexDefinition().FullName());
        }

        [Fact]
        public void NullablePropertiesAreOptional()
        {
            // NOTE: Converting this to be a data driven test is painful as we need to mock C# overload resolution algorithm.
            var builder = new ODataModelBuilder();
            var complexType = builder.ComplexType<ComplexTypeTestModel>();

            Assert.True(complexType.Property(t => t.NullableBoolProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableByteProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableDateTimeOffsetProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableDoubleProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableGuidProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableIntProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableLongProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableShortProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableTimeSpanProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableDateProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableTimeOfDayProperty).OptionalProperty);

            // Assert.True(complexType.Property(t => t.StreamProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.StringProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.ByteArrayProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.NullableDateTimeProperty).OptionalProperty);
        }

        [Fact]
        public void NonNullablePropertiesAreNotOptional()
        {
            // NOTE: Converting this to be a data driven test is painful as we need to mock C# overload resolution algorithm.
            var builder = new ODataModelBuilder();
            var complexType = builder.ComplexType<ComplexTypeTestModel>();

            Assert.False(complexType.Property(t => t.BoolProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.ByteProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.DateTimeOffsetProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.DoubleProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.GuidProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.IntProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.LongProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.ShortProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.TimeSpanProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.DateTimeProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.DateProperty).OptionalProperty);
            Assert.False(complexType.Property(t => t.TimeOfDayProperty).OptionalProperty);
        }

        [Fact]
        public void DynamicDictionaryProperty_Works_ToSetOpenComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            ComplexTypeConfiguration<SimpleOpenComplexType> complexType = builder.ComplexType<SimpleOpenComplexType>();
            complexType.Property(c => c.IntProperty);
            complexType.HasDynamicProperties(c => c.DynamicProperties);

            // Act & Assert
            Assert.True(complexType.IsOpen);
        }

        [Fact]
        public void AddDynamicDictionary_ThrowsException_IfMoreThanOneDynamicPropertyInOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<BadOpenComplexType>();

            // Act & Assert
#if NETCOREAPP3_1
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadOpenComplexType'. " +
                "Each open type must have at most one dynamic property container. (Parameter 'propertyInfo')");
#else
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadOpenComplexType'. " +
                "Each open type must have at most one dynamic property container.\r\n" +
                "Parameter name: propertyInfo");
#endif
        }


        [Fact]
        public void AnnotationDictionaryProperty_Works_ToSetOComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            ComplexTypeConfiguration<SimpleAnnotationComplexType> complexType = builder.ComplexType<SimpleAnnotationComplexType>();
            complexType.Property(c => c.IntProperty);
            complexType.HasInstanceAnnotations(c => c.InstanceAnnotations);

            // Act & Assert
            Assert.True(complexType.IsWithInstanceAnnotations);
        }

        [Fact]
        public void AddAnnotationDictionary_ThrowsException_IfMoreThanOneDynamicPropertyInComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<BadAnnotationComplexType>();

            // Act & Assert
#if NETCOREAPP3_1
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one Annotation property container in type 'BadAnnotationComplexType'. " +
                "Each open type must have at most one Annotation property container. (Parameter 'propertyInfo')");
#else
            ExceptionAssert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one Annotation property container in type 'BadAnnotationComplexType'. " +
                "Each open type must have at most one Annotation property container.\r\n" +
                "Parameter name: propertyInfo");
#endif
        }

        [Fact]
        public void GetEdmModel_WorksOnModelBuilder_ForOpenComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ComplexTypeConfiguration<SimpleAnnotationComplexType> complex = builder.ComplexType<SimpleAnnotationComplexType>();
            complex.Property(c => c.IntProperty);
            complex.HasInstanceAnnotations(c => c.InstanceAnnotations);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmComplexType complexType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            IEdmProperty edmProperty = Assert.Single(complexType.Properties());
            Assert.Equal("IntProperty", edmProperty.Name);
        }

        [Fact]
        public void GetEdmModel_WorksOnModelBuilder_ForAnnotationComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ComplexTypeConfiguration<SimpleOpenComplexType> complex = builder.ComplexType<SimpleOpenComplexType>();
            complex.Property(c => c.IntProperty);
            complex.HasDynamicProperties(c => c.DynamicProperties);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmComplexType complexType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.True(complexType.IsOpen);
            IEdmProperty edmProperty = Assert.Single(complexType.Properties());
            Assert.Equal("IntProperty", edmProperty.Name);
        }

        [Fact]
        public void GetEdmModel_WorksOnConventionModelBuilder_ForOpenComplexType()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<SimpleOpenComplexType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmComplexType complexType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.True(complexType.IsOpen);
            IEdmProperty edmProperty = Assert.Single(complexType.Properties());
            Assert.Equal("IntProperty", edmProperty.Name);
        }

        [Fact]
        public void GetEdmModel_Works_ForOpenComplexTypeWithDerivedDynamicProperty()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<OpenComplexTypeWithDerivedDynamicProperty>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmComplexType complexType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.True(complexType.IsOpen);
            IEdmProperty edmProperty = Assert.Single(complexType.Properties());
            Assert.Equal("StringProperty", edmProperty.Name);
        }

        [Fact]
        public void CanCreateAbstractComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<BaseComplexType>().Abstract();

            // Arrange
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType baseComplexType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.True(baseComplexType.IsAbstract);
        }

        [Fact]
        public void CanCreateDerivedComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<BaseComplexType>().Abstract().Property(v => v.BaseProperty);
            builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(v => v.DerivedProperty);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType baseComplexType = model.AssertHasComplexType(typeof(BaseComplexType));
            Assert.Null(baseComplexType.BaseComplexType());
            Assert.Single(baseComplexType.Properties());
            baseComplexType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);

            IEdmComplexType derivedComplexType = model.AssertHasComplexType(typeof(DerivedComplexType));
            Assert.Equal(baseComplexType, derivedComplexType.BaseComplexType());
            Assert.Equal(2, derivedComplexType.Properties().Count());
            derivedComplexType.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);
            derivedComplexType.AssertHasPrimitiveProperty(model, "DerivedProperty", EdmPrimitiveTypeKind.Int32, false);
        }

        [Fact]
        public void CanCreateDerivedComplexTypeInReverseOrder()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            model.AssertHasComplexType(typeof(BaseComplexType));
            model.AssertHasComplexType(typeof(DerivedComplexType), typeof(BaseComplexType));
        }

        [Fact]
        public void CanDefinePropertyOnDerivedType_NotPresentInBaseType_ButPresentInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(m => m.BaseProperty);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType baseComplex = model.AssertHasComplexType(typeof(BaseComplexType));
            Assert.Null(baseComplex.BaseComplexType());
            Assert.Empty(baseComplex.Properties());

            IEdmComplexType derivedComplex = model.AssertHasComplexType(typeof(DerivedComplexType));
            Assert.Equal(baseComplex, derivedComplex.BaseComplexType());
            Assert.Single(derivedComplex.Properties());
            derivedComplex.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);
        }

        [Fact]
        public void AddProperty_Throws_WhenRedefineBaseTypeProperty_OnDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(v => v.BaseProperty),
                "propertyInfo",
                "Cannot redefine property 'BaseProperty' already defined on the base type 'Microsoft.AspNet.OData.Test.Builder.BaseComplexType'.");
        }

        [Fact]
        public void AddProperty_Throws_WhenDefinePropertyOnBaseTypeAlreadyPresentInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(m => m.BaseProperty);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty),
                "propertyInfo",
                "Cannot define property 'BaseProperty' in the base type 'Microsoft.AspNet.OData.Test.Builder.BaseComplexType' " +
                "as the derived type 'Microsoft.AspNet.OData.Test.Builder.DerivedComplexType' already defines it.");
        }

        [Fact]
        public void DerivesFrom_Throws_IfDerivedTypeDoesnotDeriveFromBaseType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.ComplexType<string>().DerivesFrom<BaseComplexType>(),
                "baseType",
                "'System.String' does not inherit from 'Microsoft.AspNet.OData.Test.Builder.BaseComplexType'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInBaseType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty);
            ComplexTypeConfiguration<DerivedComplexType> derivedComplex = builder.ComplexType<DerivedComplexType>();
            derivedComplex.Property(m => m.BaseProperty);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => derivedComplex.DerivesFrom<BaseComplexType>(),
                "propertyInfo",
                "Cannot redefine property 'BaseProperty' already defined on the base type 'Microsoft.AspNet.OData.Test.Builder.BaseComplexType'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty);
            builder.ComplexType<SubDerivedComplexType>().DerivesFrom<DerivedComplexType>().Property(c => c.BaseProperty);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>(),
                "propertyInfo",
                "Cannot define property 'BaseProperty' in the base type 'Microsoft.AspNet.OData.Test.Builder.DerivedComplexType' as " +
                "the derived type 'Microsoft.AspNet.OData.Test.Builder.SubDerivedComplexType' already defines it.");
        }

        [Fact]
        public void DeriveFromItself_Throws()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => builder.EntityType<BaseComplexType>().DerivesFrom<BaseComplexType>(),
                "baseType",
                "'Microsoft.AspNet.OData.Test.Builder.BaseComplexType' does not inherit from 'Microsoft.AspNet.OData.Test.Builder.BaseComplexType'.");
        }

        [Fact]
        public void DerivesFrom_SetsBaseType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ComplexTypeConfiguration<DerivedComplexType> derivedComplex = builder.ComplexType<DerivedComplexType>();

            // Act
            derivedComplex.DerivesFrom<BaseComplexType>();

            // Assert
            Assert.NotNull(derivedComplex.BaseType);
            Assert.Equal(typeof(BaseComplexType), derivedComplex.BaseType.ClrType);
        }

        [Fact]
        public void CanDeriveFromNull()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ComplexTypeConfiguration<DerivedComplexType> derivedComplex = builder.ComplexType<DerivedComplexType>();

            // Act
            derivedComplex.DerivesFromNothing();

            // Assert
            Assert.Null(derivedComplex.BaseType);
        }

        [Fact]
        public void BaseTypeConfigured_IsFalseByDefault()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            ComplexTypeConfiguration derivedComplex = builder.AddComplexType(typeof(DerivedComplexType));

            // Assert
            Assert.False(derivedComplex.BaseTypeConfigured);
        }

        [Fact]
        public void SettingBaseType_UpdatesBaseTypeConfigured()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ComplexTypeConfiguration derivedComplex = builder.AddComplexType(typeof(DerivedComplexType));
            ComplexTypeConfiguration baseComplex = builder.AddComplexType(typeof(BaseComplexType));

            // Act
            derivedComplex.DerivesFrom(baseComplex);

            // Assert
            Assert.False(baseComplex.BaseTypeConfigured);
            Assert.True(derivedComplex.BaseTypeConfigured);
        }

        [Fact]
        public void CreateComplexTypeWith_OneToOne_NavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>().HasKey(c => c.CustomerId);

            ComplexTypeConfiguration<Order> order = builder.ComplexType<Order>();
            order.HasRequired(o => o.Customer);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType customerType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Customer", customerType.FullName());
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Single(customerType.DeclaredProperties);
            Assert.Empty(customerType.NavigationProperties());

            IEdmComplexType orderType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Order", orderType.FullName());

            IEdmNavigationProperty navProperty = Assert.Single(orderType.NavigationProperties());
            Assert.Equal(EdmMultiplicity.One, navProperty.TargetMultiplicity());
            Assert.Equal("Customer", navProperty.Name);
            Assert.True(navProperty.Type.IsEntity());
            Assert.Same(customerType, navProperty.Type.Definition);
        }

        [Fact]
        public void CreateComplexTypeWith_OneToOneOrZero_NavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>().HasKey(c => c.CustomerId);

            ComplexTypeConfiguration<Order> order = builder.ComplexType<Order>();
            order.HasOptional(o => o.Customer);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType customerType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Customer", customerType.FullName());
            Assert.Equal("CustomerId", customerType.DeclaredKey.Single().Name);
            Assert.Single(customerType.DeclaredProperties);
            Assert.Empty(customerType.NavigationProperties());

            IEdmComplexType orderType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Order", orderType.FullName());

            IEdmNavigationProperty navProperty = Assert.Single(orderType.NavigationProperties());
            Assert.Equal(EdmMultiplicity.ZeroOrOne, navProperty.TargetMultiplicity());
            Assert.Equal("Customer", navProperty.Name);
            Assert.True(navProperty.Type.IsEntity());
            Assert.Same(customerType, navProperty.Type.Definition);
        }

        [Fact]
        public void CreateComplexTypeWith_OneToMany_NavigationProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Order>().HasKey(o => o.OrderId);

            ComplexTypeConfiguration<Customer> customer = builder.ComplexType<Customer>();
            customer.HasMany(c => c.Orders);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType orderType = Assert.Single(model.SchemaElements.OfType<IEdmEntityType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Order", orderType.FullName());
            Assert.Equal("OrderId", orderType.DeclaredKey.Single().Name);
            Assert.Single(orderType.DeclaredProperties);
            Assert.Empty(orderType.NavigationProperties());

            IEdmComplexType customerType = Assert.Single(model.SchemaElements.OfType<IEdmComplexType>());
            Assert.Equal("Microsoft.AspNet.OData.Test.Builder.TestModels.Customer", customerType.FullName());

            IEdmNavigationProperty navProperty = Assert.Single(customerType.NavigationProperties());
            Assert.Equal(EdmMultiplicity.Many, navProperty.TargetMultiplicity());
            Assert.Equal("Orders", navProperty.Name);
            Assert.True(navProperty.Type.IsCollection());
            Assert.Same(orderType, navProperty.Type.AsCollection().ElementType().Definition);
        }
    }

    public class ComplexTypeTestModel
    {
        public int IntProperty { get; set; }
        public int? NullableIntProperty { get; set; }

        public short ShortProperty { get; set; }
        public short? NullableShortProperty { get; set; }

        public long LongProperty { get; set; }
        public long? NullableLongProperty { get; set; }

        public bool BoolProperty { get; set; }
        public bool? NullableBoolProperty { get; set; }

        public byte ByteProperty { get; set; }
        public byte? NullableByteProperty { get; set; }

        public double DoubleProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }

        public Guid GuidProperty { get; set; }
        public Guid? NullableGuidProperty { get; set; }

        public TimeSpan TimeSpanProperty { get; set; }
        public TimeSpan? NullableTimeSpanProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        public DateTime DateTimeProperty { get; set; }
        public DateTime? NullableDateTimeProperty { get; set; }

        public Date DateProperty { get; set; }
        public Date? NullableDateProperty { get; set; }

        public TimeOfDay TimeOfDayProperty { get; set; }
        public TimeOfDay? NullableTimeOfDayProperty { get; set; }

        public string StringProperty { get; set; }
        public Stream StreamProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
    }

    public class SimpleOpenComplexType
    {
        public int IntProperty { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class BadOpenComplexType
    {
        public int IntProperty { get; set; }
        public IDictionary<string, object> DynamicProperties1 { get; set; }
        public IDictionary<string, object> DynamicProperties2 { get; set; }
    }

    public class SimpleAnnotationComplexType
    {
        public int IntProperty { get; set; }
        public IDictionary<string, IDictionary<string, object>> InstanceAnnotations { get; set; }
    }

    public class BadAnnotationComplexType
    {
        public int IntProperty { get; set; }
        public IDictionary<string, IDictionary<string, object>> InstanceAnnotations1 { get; set; }
        public IDictionary<string, IDictionary<string, object>> InstanceAnnotations2 { get; set; }
    }

    public class MyDynamicProperty : Dictionary<string, object>
    { }

    public class OpenComplexTypeWithDerivedDynamicProperty
    {
        public string StringProperty { get; set; }
        public MyDynamicProperty MyProperties { get; set; }
    }

    public abstract class BaseComplexType
    {
        public string BaseProperty { get; set; }
    }

    public class DerivedComplexType : BaseComplexType
    {
        public int DerivedProperty { get; set; }
    }

    public class SubDerivedComplexType : DerivedComplexType
    {
        public int SubDerivedProperty { get; set; }
    }
}
