// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class DeltaTest
    {
        public static IEnumerable<object[]> DeltaModelPropertyNamesData
        {
            get
            {
                MethodInfo getDefaultValue = typeof(DeltaTest).GetMethod("GetDefaultValue");

                var defaultValues = typeof(DeltaModel).GetProperties().Select(p => new[] { p.Name, getDefaultValue.MakeGenericMethod(p.PropertyType).Invoke(obj: null, parameters: null) });
                return defaultValues.Concat(new object[][] 
                {
                    new object[] { "StringProperty" , "42" },
                    new object[] { "ComplexModelProperty", new ComplexModel { ComplexIntProperty = 42, ComplexNullableIntProperty = null } },
                    new object[] { "CollectionProperty", new Collection<int> { 1, 2, 3 }}
                });

            }
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_entityType()
        {
            Assert.ThrowsArgumentNull(() => new Delta<Base>(entityType: null), "entityType");
        }

        [Fact]
        public void Ctor_ThrowsInvalidOperation_If_EntityType_IsNotAssignable_To_TEntityType()
        {
            Assert.Throws<InvalidOperationException>(
                () => new Delta<Derived>(typeof(AnotherDerived)),
                "The entity type 'System.Web.Http.OData.DeltaTest+AnotherDerived' is not assignable to the Delta type 'System.Web.Http.OData.DeltaTest+Derived'.");
        }

        [Fact]
        public void Can_Declare_A_Delta_Of_An_AbstractClass()
        {
            Delta<AbstractBase> abstractDelta = null;
            Assert.Null(abstractDelta);
        }

        [Theory]
        [PropertyData("DeltaModelPropertyNamesData")]
        public void RoundTrip_Properties(string propertyName, object value)
        {
            Delta<DeltaModel> delta = new Delta<DeltaModel>();

            Type propertyType;
            Assert.True(delta.TryGetPropertyType(propertyName, out propertyType));

            Assert.True(delta.TrySetPropertyValue(propertyName, value));

            object retrievedValue;
            delta.TryGetPropertyValue(propertyName, out retrievedValue);
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void CanGetChangedPropertyNames()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "NY", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            var idelta = delta as IDelta;
            // modify in the way we expect the formatter too.
            idelta.TrySetPropertyValue("City", "Sammamish");
            Assert.Equal(1, idelta.GetChangedPropertyNames().Count());
            Assert.Equal("City", idelta.GetChangedPropertyNames().Single());

            // read the property back
            object city = null;
            Assert.True(idelta.TryGetPropertyValue("City", out city));
            Assert.Equal("Sammamish", city);

            // modify the way people will through custom code
            delta.StreetAddress = "23213 NE 15th Ct";
            var mods = idelta.GetChangedPropertyNames().ToArray();
            Assert.Equal(2, mods.Count());
            Assert.True(mods.Contains("StreetAddress"));
            Assert.True(mods.Contains("City"));
            Assert.Equal("23213 NE 15th Ct", delta.StreetAddress);
        }

        [Fact]
        public void CanReadUnmodifiedDefaultValuesFromDelta()
        {
            dynamic patch = new Delta<AddressEntity>();
            Assert.Equal(0, patch.ID);
            Assert.Null(patch.City);
            Assert.Null(patch.State);
            Assert.Null(patch.StreetAddress);
            Assert.Equal(0, patch.ZipCode);
        }

        [Fact]
        public void CanPatch()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";

            delta.Patch(original);
            // unchanged
            Assert.Equal(1, original.ID);
            Assert.Equal(98074, original.ZipCode);
            Assert.Equal("WA", original.State);
            // changed
            Assert.Equal("Sammamish", original.City);
            Assert.Equal("23213 NE 15th Ct", original.StreetAddress);
        }

        [Fact]
        public void CanCopyUnchangedValues()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";
            var idelta = delta as Delta<AddressEntity>;
            idelta.CopyUnchangedValues(original);
            // unchanged values have been reset to defaults
            Assert.Equal(0, original.ID);
            Assert.Equal(0, original.ZipCode);
            Assert.Equal(null, original.State);
            // changed values have been left unmodified
            Assert.Equal("Redmond", original.City);
            Assert.Equal("21110 NE 44th St", original.StreetAddress);
        }

        [Fact]
        public void CanPut()
        {
            var original = new AddressEntity { ID = 1, City = "Redmond", State = "WA", StreetAddress = "21110 NE 44th St", ZipCode = 98074 };

            dynamic delta = new Delta<AddressEntity>();
            delta.City = "Sammamish";
            delta.StreetAddress = "23213 NE 15th Ct";
            var idelta = delta as Delta<AddressEntity>;
            idelta.Put(original);

            // unchanged values have been reset to defaults
            Assert.Equal(0, original.ID);
            Assert.Equal(0, original.ZipCode);
            Assert.Equal(null, original.State);
            // changed values have been updated to values in delta
            Assert.Equal("Sammamish", original.City);
            Assert.Equal("23213 NE 15th Ct", original.StreetAddress);
        }

        [Fact]
        public void CanClear()
        {
            dynamic delta = new Delta<AddressEntity>();
            delta.StreetAddress = "Test";
            var idelta = delta as IDelta;
            Assert.Equal(1, idelta.GetChangedPropertyNames().Count());
            idelta.Clear();
            Assert.Equal(0, idelta.GetChangedPropertyNames().Count());
        }

        [Fact]
        public void CanCreateDeltaOfDerivedTypes()
        {
            var delta = new Delta<Base>(typeof(Derived));
            Assert.IsType(typeof(Derived), delta.GetEntity());
        }

        [Fact]
        public void CanChangeDerivedClassProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));

            // Act
            delta.DerivedInt = 10;

            // Assert
            Assert.Equal(delta.GetChangedPropertyNames(), new[] { "DerivedInt" });
        }

        [Fact]
        public void Patch_Patches_DerivedTypeProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 42;
            Derived derived = new Derived();

            // Act
            delta.Patch(derived);

            // Assert
            Assert.Equal(42, derived.DerivedInt);
            Assert.Equal(0, derived.BaseInt);
            Assert.Null(derived.BaseString);
            Assert.Null(derived.DerivedString);
        }

        [Fact]
        public void Put_Clears_DerivedTypeProperties()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 24;
            Derived derived = new Derived { BaseInt = 42, DerivedInt = 0, BaseString = "42", DerivedString = "42" };

            // Act
            delta.Put(derived);

            // Assert
            Assert.Equal(24, derived.DerivedInt);
            Assert.Equal(0, derived.BaseInt);
            Assert.Null(derived.BaseString);
            Assert.Null(derived.DerivedString);
        }

        [Fact]
        public void Put_DoesNotClear_PropertiesNotOnEntityType()
        {
            // Arrange
            dynamic delta = new Delta<Base>(typeof(Derived));
            delta.DerivedInt = 24;
            DerivedDerived derived = new DerivedDerived { BaseInt = 42, DerivedInt = 0, BaseString = "42", DerivedString = "42", DerivedDerivedInt = 42, DerivedDerivedString = "42" };

            // Act
            delta.Put(derived);

            // Assert
            Assert.Equal("42", derived.DerivedDerivedString);
            Assert.Equal(42, derived.DerivedDerivedInt);
        }

        [Fact]
        public void Patch_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            Assert.ThrowsArgument(
                () => delta.Patch(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'System.Web.Http.OData.DeltaTest+Derived' on an entity of type 'System.Web.Http.OData.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void Put_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            Assert.ThrowsArgument(
                () => delta.Put(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'System.Web.Http.OData.DeltaTest+Derived' on an entity of type 'System.Web.Http.OData.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void CopyChangedValues_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            Assert.ThrowsArgument(
                () => delta.CopyChangedValues(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'System.Web.Http.OData.DeltaTest+Derived' on an entity of type 'System.Web.Http.OData.DeltaTest+AnotherDerived'.");
        }

        [Fact]
        public void CopyUnchangedValues_UnRelatedType_Throws_Argument()
        {
            // Arrange
            Delta<Base> delta = new Delta<Base>(typeof(Derived));
            AnotherDerived unrelatedEntity = new AnotherDerived();

            // Act & Assert
            Assert.ThrowsArgument(
                () => delta.CopyUnchangedValues(unrelatedEntity),
                "original",
                "Cannot use Delta of type 'System.Web.Http.OData.DeltaTest+Derived' on an entity of type 'System.Web.Http.OData.DeltaTest+AnotherDerived'.");
        }

        public static TheoryDataSet<string, string, object> ODataFormatter_Can_Read_Delta_DataSet
        {
            get
            {
                return new TheoryDataSet<string, string, object>()
                {
                    { "IntProperty", "23", 23 },
                    { "LongProperty", String.Format(CultureInfo.InvariantCulture, "'{0}'", Int64.MaxValue), Int64.MaxValue }, // longs are serialized as strings in odata json
                    { "LongProperty", String.Format(CultureInfo.InvariantCulture, "'{0}'", Int64.MinValue), Int64.MinValue }, // longs are serialized as strings in odata json
                    { "NullableIntProperty", "null", null },
                    { "BoolProperty", "true", true },
                    { "NullableBoolProperty", "null", null },
                    { "DateTimeProperty", "'\\/Date(694224000000)\\/'", new DateTime(1992, 1, 1) },
                    { "StringProperty", "'42'", "42" },
                    { "ComplexModelProperty", "{ 'ComplexIntProperty' : 42 }", new ComplexModel { ComplexIntProperty = 42 } },
                    { "CollectionProperty", "{ 'results' : [ 1, 2, 3 ] }", new Collection<int> { 1,2, 3} },
                    { "ComplexModelCollectionProperty", "{ 'results' : [ { 'ComplexIntProperty' : 42 } ] }", new Collection<ComplexModel> { new ComplexModel { ComplexIntProperty = 42 } } }
                };
            }
        }

        [Theory]
        [PropertyData("ODataFormatter_Can_Read_Delta_DataSet")]
        public void ODataFormatter_Can_Read_Delta(string propertyName, string propertyJsonValue, object expectedValue)
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Entity<DeltaModel>();
            builder.EntitySet<DeltaModel>("ignored");
            IEdmModel model = builder.GetEdmModel();
            IEnumerable<ODataMediaTypeFormatter> odataFormatters = ODataMediaTypeFormatters.Create();
            Delta<DeltaModel> delta;

            using (HttpRequestMessage request = new HttpRequestMessage { RequestUri = new Uri("http://localhost") })
            {
                IEdmEntitySet entitySet = model.EntityContainers().Single().EntitySets().Single();
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapODataRoute("default", "", model);
                request.SetODataRouteName("default");
                request.SetConfiguration(config);
                request.SetEdmModel(model);
                request.SetODataPath(new ODataPath(new EntitySetPathSegment(entitySet)));
                IEnumerable<MediaTypeFormatter> perRequestFormatters = odataFormatters.Select(
                    (f) => f.GetPerRequestFormatterInstance(typeof(Delta<DeltaModel>), request, null));

                HttpContent content = new StringContent(String.Format("{{ '{0}' : {1} }}", propertyName, propertyJsonValue));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

                // Act
                delta = content.ReadAsAsync<Delta<DeltaModel>>(perRequestFormatters).Result;
            }

            // Assert
            Assert.Equal(delta.GetChangedPropertyNames(), new[] { propertyName });
            object value;
            Assert.True(delta.TryGetPropertyValue(propertyName, out value));
            Assert.Equal(expectedValue, value);
        }

        public static TheoryDataSet<Type> Delta_Returns_Correct_ExpectedType_And_ActualType_DataSet
        {
            get
            {
                return new TheoryDataSet<Type>()
                {
                    { typeof(Customer) },
                    { typeof(BellevueCustomer) } 
                };
            }
        }

        [Theory]
        [PropertyData("Delta_Returns_Correct_ExpectedType_And_ActualType_DataSet")]
        public void Delta_Returns_Correct_ExpectedType_And_ActualType(Type actualType)
        { 
            // Arrange 
            Delta delta = new Delta<Customer>(actualType);

            // Act
            Type actualActualType = delta.EntityType;
            Type actualExpectedType = delta.ExpectedType;

            // Assert
            Assert.Equal(typeof(Customer), actualExpectedType);
            Assert.Equal(actualType, actualActualType);
        }

        public static T GetDefaultValue<T>()
        {
            return default(T);
        }

        private class DeltaModel
        {
            public int IntProperty { get; set; }

            public int? NullableIntProperty { get; set; }

            public long LongProperty { get; set; }

            public long? NullableLongProperty { get; set; }

            public bool BoolProperty { get; set; }

            public bool? NullableBoolProperty { get; set; }

            public Guid GuidProperty { get; set; }

            public Guid? NullableGuidProperty { get; set; }

            public DateTime DateTimeProperty { get; set; }

            public DateTime? NullableDateTimeProperty { get; set; }

            public string StringProperty { get; set; }

            public ComplexModel ComplexModelProperty { get; set; }

            public Collection<int> CollectionProperty { get; set; }

            public Collection<ComplexModel> ComplexModelCollectionProperty { get; set; }
        }

        private class ComplexModel
        {
            public int ComplexIntProperty { get; set; }

            public int? ComplexNullableIntProperty { get; set; }

            public override bool Equals(object obj)
            {
                ComplexModel model = obj as ComplexModel;

                if (model == null)
                {
                    return false;
                }

                return ComplexIntProperty == model.ComplexIntProperty && ComplexNullableIntProperty == model.ComplexNullableIntProperty;
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }

        private abstract class AbstractBase
        {
        }

        private class Base
        {
            public int BaseInt { get; set; }

            public string BaseString { get; set; }
        }

        private class Derived : Base
        {
            public int DerivedInt { get; set; }

            public string DerivedString { get; set; }
        }

        private class DerivedDerived : Derived
        {
            public int DerivedDerivedInt { get; set; }

            public string DerivedDerivedString { get; set; }
        }

        private class AnotherDerived : Base
        {
        }
    }
}
