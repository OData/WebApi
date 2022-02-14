//-----------------------------------------------------------------------------
// <copyright file="BindingPathConfigurationOfTStructuralTypeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class BindingPathConfigurationOfTStructuralTypeTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_ModelBuilder()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new BindingPathConfiguration<object>(null, structuralType: null, navigationSource: null),
                "modelBuilder");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_StructuralType()
        {
            // Assert
            Mock<ODataModelBuilder> builder = new Mock<ODataModelBuilder>();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new BindingPathConfiguration<object>(builder.Object, structuralType: null, navigationSource: null),
                "structuralType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_NavigationSource()
        {
            // Assert
            Mock<ODataModelBuilder> builder = new Mock<ODataModelBuilder>();
            ComplexTypeConfiguration complex = new ComplexTypeConfiguration();
            ComplexTypeConfiguration<object> structuralType = new ComplexTypeConfiguration<object>(complex);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new BindingPathConfiguration<object>(builder.Object, structuralType, navigationSource: null),
                "navigationSource");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_BindingPath()
        {
            // Assert
            Mock<ODataModelBuilder> builder = new Mock<ODataModelBuilder>();
            ComplexTypeConfiguration complex = new ComplexTypeConfiguration();
            ComplexTypeConfiguration<object> structuralType = new ComplexTypeConfiguration<object>(complex);
            EntitySetConfiguration navigationSource = new EntitySetConfiguration();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () =>
                    new BindingPathConfiguration<object>(builder.Object, structuralType, navigationSource,
                        bindingPath: null), "bindingPath");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HasManyPath_AddBindindPath(bool contained)
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard

            // Act
            var binding = new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration);
            var newBinding = binding.HasManyPath(c => c.Addresses, contained);

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            PropertyConfiguration addressesProperty = Assert.Single(customerType.Properties);
            Assert.Equal("Addresses", addressesProperty.Name);

            if (contained)
            {
                Assert.Equal(EdmTypeKind.Entity, addressType.Kind);
                Assert.Equal(PropertyKind.Navigation, addressesProperty.Kind);
                NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(addressesProperty);
                Assert.Equal(EdmMultiplicity.Many, navigationProperty.Multiplicity);
                Assert.True(navigationProperty.ContainsTarget);
            }
            else
            {
                Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
                Assert.Equal(PropertyKind.Collection, addressesProperty.Kind);
                CollectionPropertyConfiguration collection = Assert.IsType<CollectionPropertyConfiguration>(addressesProperty);
                Assert.Equal(typeof(BindingAddress), collection.ElementType);
            }

            // different bindings
            Assert.NotSame(binding, newBinding);
            Assert.Equal("", binding.BindingPath);

            Assert.IsType<BindingPathConfiguration<BindingAddress>>(newBinding);
            Assert.Equal("Addresses", newBinding.BindingPath);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HasManyPath_AddBindindPath_Derived(bool contained)
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard

            // Act
            var binding = new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration);
            var newBinding = binding.HasManyPath((BindingVipCustomer v) => v.VipAddresses, contained);

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            Assert.Empty(customerType.Properties);

            StructuralTypeConfiguration vipCustomerType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingVipCustomer");
            Assert.NotNull(vipCustomerType);
            var vipAddressesProperty = Assert.Single(vipCustomerType.Properties);
            Assert.Equal("VipAddresses", vipAddressesProperty.Name);

            if (contained)
            {
                Assert.Equal(EdmTypeKind.Entity, addressType.Kind);
                Assert.Equal(PropertyKind.Navigation, vipAddressesProperty.Kind);
                NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(vipAddressesProperty);
                Assert.Equal(EdmMultiplicity.Many, navigationProperty.Multiplicity);
                Assert.True(navigationProperty.ContainsTarget);
            }
            else
            {
                Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
                Assert.Equal(PropertyKind.Collection, vipAddressesProperty.Kind);
                CollectionPropertyConfiguration collection = Assert.IsType<CollectionPropertyConfiguration>(vipAddressesProperty);
                Assert.Equal(typeof(BindingAddress), collection.ElementType);
            }

            // different bindings
            Assert.NotSame(binding, newBinding);
            Assert.Equal("", binding.BindingPath);

            Assert.IsType<BindingPathConfiguration<BindingAddress>>(newBinding);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses", newBinding.BindingPath);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void HasSinglePath_AddBindindPath(bool required, bool contained)
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard

            // Act
            var binding = new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration);
            var newBinding = binding.HasSinglePath(c => c.Location, required, contained);

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            PropertyConfiguration locationProperty = Assert.Single(customerType.Properties);
            Assert.Equal("Location", locationProperty.Name);

            if (contained)
            {
                Assert.Equal(EdmTypeKind.Entity, addressType.Kind);
                Assert.Equal(PropertyKind.Navigation, locationProperty.Kind);
                NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(locationProperty);
                if (required)
                {
                    Assert.Equal(EdmMultiplicity.One, navigationProperty.Multiplicity);
                }
                else
                {
                    Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);
                }

                Assert.True(navigationProperty.ContainsTarget);
            }
            else
            {
                Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
                Assert.Equal(PropertyKind.Complex, locationProperty.Kind);
                ComplexPropertyConfiguration complexProperty = Assert.IsType<ComplexPropertyConfiguration>(locationProperty);
                Assert.Equal(!required, complexProperty.OptionalProperty);
                Assert.Equal(typeof(BindingAddress), complexProperty.RelatedClrType);
            }

            // different bindings
            Assert.NotSame(binding, newBinding);
            Assert.Equal("", binding.BindingPath);

            Assert.IsType<BindingPathConfiguration<BindingAddress>>(newBinding);
            Assert.Equal("Location", newBinding.BindingPath);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void HasSinglePath_AddBindindPath_Derived(bool required, bool contained)
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard

            // Act
            var binding = new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration);
            var newBinding = binding.HasSinglePath((BindingVipCustomer v) => v.VipLocation, required, contained);

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            Assert.Empty(customerType.Properties);

            StructuralTypeConfiguration vipCustomerType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingVipCustomer");
            Assert.NotNull(vipCustomerType);
            var vipLocationProperty = Assert.Single(vipCustomerType.Properties);
            Assert.Equal("VipLocation", vipLocationProperty.Name);

            if (contained)
            {
                Assert.Equal(EdmTypeKind.Entity, addressType.Kind);
                Assert.Equal(PropertyKind.Navigation, vipLocationProperty.Kind);
                NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(vipLocationProperty);
                if (required)
                {
                    Assert.Equal(EdmMultiplicity.One, navigationProperty.Multiplicity);
                }
                else
                {
                    Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);
                }

                Assert.True(navigationProperty.ContainsTarget);
            }
            else
            {
                Assert.Equal(EdmTypeKind.Complex, addressType.Kind);
                Assert.Equal(PropertyKind.Complex, vipLocationProperty.Kind);
                ComplexPropertyConfiguration complexProperty = Assert.IsType<ComplexPropertyConfiguration>(vipLocationProperty);
                Assert.Equal(!required, complexProperty.OptionalProperty);
                Assert.Equal(typeof(BindingAddress), complexProperty.RelatedClrType);
            }

            // different bindings
            Assert.NotSame(binding, newBinding);
            Assert.Equal("", binding.BindingPath);

            Assert.IsType<BindingPathConfiguration<BindingAddress>>(newBinding);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation", newBinding.BindingPath);
        }

        [Fact]
        public void HasManyBinding_AddBindindToNavigationSource()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_A")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasManyPath(c => c.Addresses)
                .HasManyBinding(a => a.Cities, "Cities_A");

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            PropertyConfiguration citiesProperty = Assert.Single(addressType.Properties);
            Assert.Equal("Cities", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.Many, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_A", binding.TargetNavigationSource.Name);
            Assert.Equal("Addresses/Cities", binding.BindingPath);
        }

        [Fact]
        public void HasManyBinding_AddBindindToNavigationSource_Derived()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_B")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasManyPath((BindingVipCustomer v) => v.VipAddresses)
                .HasManyBinding((BindingUsAddress u) => u.UsCities, "Cities_B");

            // Assert
            var usAddressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingUsAddress");
            Assert.NotNull(usAddressType);
            PropertyConfiguration citiesProperty = Assert.Single(usAddressType.Properties);
            Assert.Equal("UsCities", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.Many, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_B", binding.TargetNavigationSource.Name);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipAddresses/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCities", binding.BindingPath);
        }

        [Fact]
        public void HasRequiredBinding_AddBindindToNavigationSource()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_C")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasSinglePath(c => c.Location)
                .HasRequiredBinding(a => a.City, "Cities_C");

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            PropertyConfiguration citiesProperty = Assert.Single(addressType.Properties);
            Assert.Equal("City", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.One, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_C", binding.TargetNavigationSource.Name);
            Assert.Equal("Location/City", binding.BindingPath);
        }

        [Fact]
        public void HasRequiredBinding_AddBindindToNavigationSource_Derived()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_D")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasSinglePath((BindingVipCustomer v) => v.VipLocation)
                .HasRequiredBinding((BindingUsAddress u) => u.UsCity, "Cities_D");

            // Assert
            var usAddressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingUsAddress");
            Assert.NotNull(usAddressType);
            PropertyConfiguration citiesProperty = Assert.Single(usAddressType.Properties);
            Assert.Equal("UsCity", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.One, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_D", binding.TargetNavigationSource.Name);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity", binding.BindingPath);
        }

        [Fact]
        public void HasOptionalBinding_AddBindindToNavigationSource()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_C")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasSinglePath(c => c.Location)
                .HasOptionalBinding(a => a.City, "Cities_C");

            // Assert
            addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.NotNull(addressType);
            PropertyConfiguration citiesProperty = Assert.Single(addressType.Properties);
            Assert.Equal("City", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_C", binding.TargetNavigationSource.Name);
            Assert.Equal("Location/City", binding.BindingPath);
        }

        [Fact]
        public void HasOptionalBinding_AddBindindToNavigationSource_Derived()
        {
            // Assert
            ODataModelBuilder builder = new ODataModelBuilder();
            var customerType = builder.EntityType<BindingCustomer>();
            var navigationSource = builder.EntitySet<BindingCustomer>("Customers");

            StructuralTypeConfiguration addressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingAddress");
            Assert.Null(addressType); // Guard
            Assert.Empty(customerType.Properties); // Guard
            Assert.Null(builder.EntitySets.FirstOrDefault(e => e.Name == "Cities_D")); // Guard

            // Act
            new BindingPathConfiguration<BindingCustomer>(builder, customerType, navigationSource.Configuration)
                .HasSinglePath((BindingVipCustomer v) => v.VipLocation)
                .HasOptionalBinding((BindingUsAddress u) => u.UsCity, "Cities_D");

            // Assert
            var usAddressType = builder.StructuralTypes.FirstOrDefault(c => c.Name == "BindingUsAddress");
            Assert.NotNull(usAddressType);
            PropertyConfiguration citiesProperty = Assert.Single(usAddressType.Properties);
            Assert.Equal("UsCity", citiesProperty.Name);

            NavigationPropertyConfiguration navigationProperty = Assert.IsType<NavigationPropertyConfiguration>(citiesProperty);
            Assert.Equal(EdmMultiplicity.ZeroOrOne, navigationProperty.Multiplicity);

            var bindings = navigationSource.FindBinding(navigationProperty);
            var binding = Assert.Single(bindings);
            Assert.Equal("Cities_D", binding.TargetNavigationSource.Name);
            Assert.Equal("Microsoft.AspNet.OData.Test.Formatter.BindingVipCustomer/VipLocation/Microsoft.AspNet.OData.Test.Formatter.BindingUsAddress/UsCity", binding.BindingPath);
        }
    }
}
