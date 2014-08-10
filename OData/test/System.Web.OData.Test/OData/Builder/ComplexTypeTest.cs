// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.OData.Builder.TestModels;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
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
        [Trait("Description", "ODataModelBuilder can't create nested complex type properties (that infinitely recurse)")]
        public void CreateInfiniteRecursiveComplexTypeDefinitionFails()
        {
            var builder = new ODataModelBuilder()
                .Add_RecursiveZipCode_ComplexType();

            var zipCode = builder.ComplexType<RecursiveZipCode>();

            Assert.ThrowsArgument(
                () => zipCode.ComplexProperty(z => z.Recursive),
                "propertyInfo",
                "The complex type 'System.Web.OData.Builder.TestModels.RecursiveZipCode' has a reference to itself through the property 'Recursive'. A recursive loop of complex types is not allowed.");
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

            // Assert.True(complexType.Property(t => t.StreamProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.StringProperty).OptionalProperty);
            Assert.True(complexType.Property(t => t.ByteArrayProperty).OptionalProperty);
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<BadOpenComplexType>();

            // Act & Assert
            Assert.ThrowsArgument(() => builder.GetEdmModel(),
                "propertyInfo",
                "Found more than one dynamic property container in type 'BadOpenComplexType'. " +
                "Each open type must have at most one dynamic property container.\r\n" +
                "Parameter name: propertyInfo");
        }

        [Fact]
        public void GetEdmModel_WorksOnModelBuilder_ForOpenComplexType()
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
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
            Assert.Equal(1, baseComplexType.Properties().Count());
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
            Assert.Equal(0, baseComplex.Properties().Count());

            IEdmComplexType derivedComplex = model.AssertHasComplexType(typeof(DerivedComplexType));
            Assert.Equal(baseComplex, derivedComplex.BaseComplexType());
            Assert.Equal(1, derivedComplex.Properties().Count());
            derivedComplex.AssertHasPrimitiveProperty(model, "BaseProperty", EdmPrimitiveTypeKind.String, true);
        }

        [Fact]
        public void AddProperty_Throws_WhenRedefineBaseTypeProperty_OnDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty);

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(v => v.BaseProperty),
                "propertyInfo",
                "Cannot redefine property 'BaseProperty' already defined on the base type 'System.Web.OData.Builder.BaseComplexType'.");
        }

        [Fact]
        public void AddProperty_Throws_WhenDefinePropertyOnBaseTypeAlreadyPresentInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>().Property(m => m.BaseProperty);

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty),
                "propertyInfo",
                "Cannot define property 'BaseProperty' in the base type 'System.Web.OData.Builder.BaseComplexType' " +
                "as the derived type 'System.Web.OData.Builder.DerivedComplexType' already defines it.");
        }

        [Fact]
        public void DerivesFrom_Throws_IfDerivedTypeDoesnotDeriveFromBaseType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.ComplexType<string>().DerivesFrom<BaseComplexType>(),
                "baseType",
                "'System.String' does not inherit from 'System.Web.OData.Builder.BaseComplexType'.");
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
            Assert.ThrowsArgument(
                () => derivedComplex.DerivesFrom<BaseComplexType>(),
                "propertyInfo",
                "Cannot redefine property 'BaseProperty' already defined on the base type 'System.Web.OData.Builder.BaseComplexType'.");
        }

        [Fact]
        public void DerivesFrom_Throws_WhenSettingTheBaseType_IfDuplicatePropertyInDerivedType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            builder.ComplexType<BaseComplexType>().Property(v => v.BaseProperty);
            builder.ComplexType<SubDerivedComplexType>().DerivesFrom<DerivedComplexType>().Property(c => c.BaseProperty);

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.ComplexType<DerivedComplexType>().DerivesFrom<BaseComplexType>(),
                "propertyInfo",
                "Cannot define property 'BaseProperty' in the base type 'System.Web.OData.Builder.DerivedComplexType' as " +
                "the derived type 'System.Web.OData.Builder.SubDerivedComplexType' already defines it.");
        }

        [Fact]
        public void DeriveFromItself_Throws()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            Assert.ThrowsArgument(
                () => builder.EntityType<BaseComplexType>().DerivesFrom<BaseComplexType>(),
                "baseType",
                "'System.Web.OData.Builder.BaseComplexType' does not inherit from 'System.Web.OData.Builder.BaseComplexType'.");
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
