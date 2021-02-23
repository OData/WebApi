// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class StructuralTypeConfigurationTest
    {
        private StructuralTypeConfiguration _configuration;
        private string _name = "name";
        private string _namespace = "com.contoso";

        public StructuralTypeConfigurationTest()
        {
            Mock<StructuralTypeConfiguration> mockConfiguration = new Mock<StructuralTypeConfiguration> { CallBase = true };
            mockConfiguration.Object.Name = "Name";
            mockConfiguration.Object.Namespace = "Namespace";
            _configuration = mockConfiguration.Object;
        }

        [Fact]
        public void Property_Name_RoundTrips()
        {
            ReflectionAssert.Property(_configuration, c => c.Name, "Name", allowNull: false, roundTripTestValue: _name);
        }

        [Fact]
        public void Property_Namespace_RoundTrips()
        {
            ReflectionAssert.Property(_configuration, c => c.Namespace, "Namespace", allowNull: false, roundTripTestValue: _namespace);
        }

        [Fact]
        public void Property_AddedExplicitly_RoundTrips()
        {
            ReflectionAssert.BooleanProperty(_configuration, c => c.AddedExplicitly, true);
        }

        [Fact]
        public void AddDynamicPropertyDictionary_ThrowsIfTypeIsNotDictionary()
        {
            // Arrange
            MockPropertyInfo property = new MockPropertyInfo(typeof(Int32), "Test");
            Mock<StructuralTypeConfiguration> mock = new Mock<StructuralTypeConfiguration> { CallBase = true };
            StructuralTypeConfiguration configuration = mock.Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => configuration.AddDynamicPropertyDictionary(property),
                "propertyInfo",
                string.Format("The argument must be of type '{0}'.", "IDictionary<string, object>"));
        }

        [Fact]
        public void AddInstanceAnnotationDictionary_ThrowsIfTypeIsNotDictionary()
        {
            // Arrange
            MockPropertyInfo property = new MockPropertyInfo(typeof(Int32), "Test");
            Mock<StructuralTypeConfiguration> mock = new Mock<StructuralTypeConfiguration> { CallBase = true };
            StructuralTypeConfiguration configuration = mock.Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => configuration.AddInstanceAnnotationContainer(property),
                "propertyInfo", string.Format(SRResources.PropertyTypeShouldBeOfType, "IODataInstanceAnnotationContainer"));
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that user assigned namespaces are honored during registration.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToStructuralType_AssignedNamespace()
        {
            // Arrange & Act.
            string expectedNamespace = "TestingNamespace";
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder()
            {
                Namespace = expectedNamespace
            };

            modelBuilder.EntitySet<Client>("clients");
            modelBuilder.ComplexType<ZipCode>();

            // Assert
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<Client>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<MyOrder>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderLine>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderHeader>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.ComplexType<ZipCode>().Namespace);
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that user assigned namespaces are honored during registration.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_SettingNamespaceToNullOrEmptyRevertsItToDefault()
        {
            // Arrange & Act.
            ODataConventionModelBuilder modelBuilderWithNullNamespace = new ODataConventionModelBuilder()
            {
                Namespace = null
            };

            // Assert
            Assert.Equal("Default", modelBuilderWithNullNamespace.Namespace);


            ODataConventionModelBuilder modelBuilderWithEmptyNamespace = new ODataConventionModelBuilder()
            {
                Namespace = string.Empty
            };

            // Assert
            Assert.Equal("Default", modelBuilderWithEmptyNamespace.Namespace);
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that user assigned namespaces are honored during registration
        /// but individual types can have namespaces assigned to them as well.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToStructuralType_AssignedNamespaceAndClassNamespace()
        {
            // Arrange & Act.
            string expectedNamespace = "TestingNamespace";
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder()
            {
                Namespace = expectedNamespace
            };

            modelBuilder.EntitySet<Client>("clients");
            modelBuilder.EntitySet<MyOrder>("orders").EntityType.Namespace = "DifferentNamespace";
            modelBuilder.ComplexType<ZipCode>();

            // Assert
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<Client>().Namespace);
            Assert.Equal("DifferentNamespace", modelBuilder.EntityType<MyOrder>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderLine>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderHeader>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.ComplexType<ZipCode>().Namespace);
        }

        /// <summary>
        /// Tests the namespace assignment logic to ensure that default CLR namespace is used if user does not assign one.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToStructuralType_DefaultNamespace()
        {
            // Arrange & Act.
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Client>("clients");
            modelBuilder.ComplexType<ZipCode>();

            // Assert
            Assert.Equal(typeof(Client).Namespace, modelBuilder.EntityType<Client>().Namespace);
            Assert.Equal(typeof(MyOrder).Namespace, modelBuilder.EntityType<MyOrder>().Namespace);
            Assert.Equal(typeof(OrderLine).Namespace, modelBuilder.EntityType<OrderLine>().Namespace);
            Assert.Equal(typeof(OrderHeader).Namespace, modelBuilder.EntityType<OrderHeader>().Namespace);
            Assert.Equal(typeof(ZipCode).Namespace, modelBuilder.ComplexType<ZipCode>().Namespace);
        }

        /// <summary>
        /// Tests the namespace assignment logic to check the order in which namespace is assigned matters.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_AutoAssignsNamespaceToStructuralType_NamespaceAssignedAfterAddingEntities()
        {
            // Arrange & Act.
            string expectedNamespace = "TestingNamespace";
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();

            modelBuilder.EntitySet<Client>("clients");

            modelBuilder.Namespace = expectedNamespace;
            modelBuilder.ComplexType<ZipCode>();

            // Assert
            // Client was registered explicitly so picks up the namespace. Auto discovery is done at model generation hence uses the assigned namespace
            Assert.Equal(typeof(Client).Namespace, modelBuilder.EntityType<Client>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<MyOrder>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderLine>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.EntityType<OrderHeader>().Namespace);
            Assert.Equal(expectedNamespace, modelBuilder.ComplexType<ZipCode>().Namespace);
        }

        /// <summary>
        /// Tests the full name property getter logic with an empty namespace to ensure the full name doesn't begin with a period.
        /// </summary>
        [Fact]
        public void NamespaceAssignment_WithEmptyNamespace_FullNameDoesNotBeginWithPeriod()
        {
            // Arrange & Act.
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();

            EntityTypeConfiguration<MyOrder> entityTypeConfiguration = modelBuilder.EntitySet<MyOrder>("orders").EntityType;
            entityTypeConfiguration.Namespace = string.Empty;

            // Assert
            Assert.Equal("MyOrder", entityTypeConfiguration.FullName);
        }
    }
}
