// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Builder
{
    public class ODataModelBuilderTest
    {
        [Fact]
        public void RemoveStructuralType_RemovesComplexType()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddComplexType(typeof(Customer));

            Assert.NotEmpty(builder.StructuralTypes);

            builder.RemoveStructuralType(typeof(Customer));
            Assert.Empty(builder.StructuralTypes);
        }

        [Fact]
        public void RemoveStructuralType_RemovesEntityType()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.AddEntity(typeof(Customer));

            Assert.NotEmpty(builder.StructuralTypes);

            builder.RemoveStructuralType(typeof(Customer));
            Assert.Empty(builder.StructuralTypes);
        }

        [Fact]
        public void CanRemoveProcedureByName()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("Format");
            bool removed = builder.RemoveProcedure("Format");

            // Assert      
            Assert.Equal(0, builder.Procedures.Count());
        }

        [Fact]
        public void CanRemoveProcedure()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration action = builder.Action("Format");
            ProcedureConfiguration procedure = builder.Procedures.SingleOrDefault();
            bool removed = builder.RemoveProcedure(procedure);

            // Assert
            Assert.True(removed);
            Assert.Equal(0, builder.Procedures.Count());
        }

        [Fact]
        public void RemoveProcedureByNameThrowsWhenAmbiguous()
        {
            // Arrange
            // Act
            ODataModelBuilder builder = new ODataModelBuilder();

            ActionConfiguration action1 = builder.Action("Format");
            ActionConfiguration action2 = builder.Action("Format");
            action2.Parameter<int>("SegmentSize");

            Assert.Throws<InvalidOperationException>(() =>
            {
                builder.RemoveProcedure("Format");
            });
        }

        [Fact]
        public void BuilderIncludesMapFromEntityTypeToBindableProcedures()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Name);
            customer.Action("Reward");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().SingleOrDefault();

            // Act
            BindableProcedureFinder finder = model.GetAnnotationValue<BindableProcedureFinder>(model);

            // Assert
            Assert.NotNull(finder);
            Assert.NotNull(finder.FindProcedures(customerType).SingleOrDefault());
            Assert.Equal("Reward", finder.FindProcedures(customerType).SingleOrDefault().Name);
        }

        [Fact]
        public void DataServiceVersion_RoundTrips()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            Assert.Reflection.Property(builder, b => b.DataServiceVersion, new Version(3, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void MaxDataServiceVersion_RoundTrips()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            Assert.Reflection.Property(builder, b => b.MaxDataServiceVersion, new Version(3, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void DataServiceVersion_Is_AppliedToTheResultingModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.DataServiceVersion = new Version(2, 2);

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(new Version(2, 2), model.GetDataServiceVersion());
        }

        [Fact]
        public void MaxDataServiceVersion_Is_AppliedToTheResultingModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.MaxDataServiceVersion = new Version(2, 2);

            IEdmModel model = builder.GetEdmModel();

            Assert.Equal(new Version(2, 2), model.GetMaxDataServiceVersion());
        }

        [Fact]
        public void EntityContainer_Is_Default()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            IEdmModel model = builder.GetEdmModel();

            Assert.True(model.IsDefaultEntityContainer(model.SchemaElements.OfType<IEdmEntityContainer>().Single()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ActionLink_PreservesFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            ActionConfiguration configuration = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> bindingParameterTypeMock = new Mock<IEdmTypeConfiguration>();
            bindingParameterTypeMock.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            Type entityType = typeof(object);
            bindingParameterTypeMock.Setup(o => o.ClrType).Returns(entityType);
            configuration.SetBindingParameter("IgnoreParameter", bindingParameterTypeMock.Object,
                alwaysBindable: false);
            configuration.HasActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);
            builder.AddProcedure(configuration);
            builder.AddEntity(entityType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmFunctionImport functionImport =
                model.EntityContainers().Single().Elements.OfType<IEdmFunctionImport>().Single();
            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(functionImport);
            Assert.Equal(value, actionLinkBuilder.FollowsConventions);
        }

        [Fact]
        public void GetEdmModel_PropertyWithDatabaseAttribute_SetStoreGeneratedPatternOnEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Entity<Customer>().Property(c => c.Name).HasStoreGeneratedPattern(DatabaseGeneratedOption.Computed);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType type = model.AssertHasEntityType(typeof(Customer));
            IEdmStructuralProperty property = type.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            var idAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                property,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Equal(DatabaseGeneratedOption.Computed.ToString(), idAnnotation.Value);
        }

        [Fact]
        public void GetEdmModel_PropertyWithDatabaseAttribute_CannotSetStoreGeneratedPatternOnComplexType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.ComplexType<Customer>().Property(c => c.Name).HasStoreGeneratedPattern(DatabaseGeneratedOption.Computed);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType type = model.AssertHasComplexType(typeof(Customer));
            IEdmStructuralProperty property = type.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            var idAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                property,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Null(idAnnotation);
        }

        [Fact]
        public void GetEdmModel_PropertyWithDatabaseAttribute_ConfigAnnotationOnPropertyOnEntityType()
        {
            // Arrange
            MockType type =
                new MockType("Entity")
                .Property(typeof(int), "ID", new DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity))
                .Property(typeof(int?), "Count");

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddEntity(type);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType entity = model.AssertHasEntityType(type);
            IEdmStructuralProperty idProperty = entity.AssertHasPrimitiveProperty(model, "ID",
                EdmPrimitiveTypeKind.Int32, isNullable: false);

            var idAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                idProperty,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Equal(DatabaseGeneratedOption.Identity.ToString(), idAnnotation.Value);

            IEdmStructuralProperty countProperty = entity.AssertHasPrimitiveProperty(model, "Count",
                EdmPrimitiveTypeKind.Int32, isNullable: true);

            var countAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                countProperty,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Null(countAnnotation);
        }

        [Fact]
        public void GetEdmModel_PropertyWithDatabaseAttribute_CannotConfigAnnotationOnPropertyOnComplexType()
        {
            // Arrange
            MockType type =
                new MockType("Complex")
                    .Property(typeof(int), "ID", new DatabaseGeneratedAttribute(DatabaseGeneratedOption.Computed))
                    .Property(typeof(int?), "Count");

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.AddComplexType(type);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmComplexType entity = model.AssertHasComplexType(type);
            IEdmStructuralProperty idProperty = entity.AssertHasPrimitiveProperty(model, "ID",
                EdmPrimitiveTypeKind.Int32, isNullable: false);

            var idAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                idProperty,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Null(idAnnotation);

            IEdmStructuralProperty countProperty = entity.AssertHasPrimitiveProperty(model, "Count",
                EdmPrimitiveTypeKind.Int32, isNullable: true);

            var countAnnotation = model.GetAnnotationValue<EdmStringConstant>(
                countProperty,
                StoreGeneratedPatternAnnotation.AnnotationsNamespace,
                StoreGeneratedPatternAnnotation.AnnotationName);
            Assert.Null(countAnnotation);
        }

        [Fact]
        public void HasOptional_CanSetSingleForeignKeyProperty_ForReferentialConstraint()
        {
            // Arrange
            PropertyInfo expectPrincipalPropertyInfo = typeof(User).GetProperty("UserId");
            PropertyInfo expectDependentPropertyInfo = typeof(Role).GetProperty("UserForeignKey");
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.Entity<Role>().HasOptional(r => r.User, (r, u) => r.UserForeignKey == u.UserId);

            // Assert
            PropertyInfo actualDependentPropertyInfo = Assert.Single(navigationProperty.DependentProperties);
            Assert.Same(expectDependentPropertyInfo, actualDependentPropertyInfo);

            PropertyInfo actualPrincipalPropertyInfo = Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Same(expectPrincipalPropertyInfo, actualPrincipalPropertyInfo);

            PropertyConfiguration propertyConfiguration = Assert.Single(navigationProperty.DeclaringEntityType.Properties
                .Where(e => e.PropertyInfo == actualDependentPropertyInfo));

            PrimitivePropertyConfiguration primitiveProperty =
                Assert.IsType<PrimitivePropertyConfiguration>(propertyConfiguration);
            Assert.False(primitiveProperty.OptionalProperty);
        }

        [Fact]
        public void HasRequired_CanSetSingleForeignKeyProperty_ForReferentialConstraint()
        {
            // Arrange
            PropertyInfo expectPrincipalPropertyInfo = typeof(User).GetProperty("UserId");
            PropertyInfo expectDependentPropertyInfo = typeof(Role).GetProperty("UserForeignKey");
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.Entity<Role>().HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.UserId);

            // Assert
            PropertyInfo actualDependentPropertyInfo = Assert.Single(navigationProperty.DependentProperties);
            Assert.Same(expectDependentPropertyInfo, actualDependentPropertyInfo);

            PropertyInfo actualPrincipalPropertyInfo = Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Same(expectPrincipalPropertyInfo, actualPrincipalPropertyInfo);

            PropertyConfiguration propertyConfiguration = Assert.Single(navigationProperty.DeclaringEntityType.Properties
                .Where(e => e.PropertyInfo == actualDependentPropertyInfo));

            PrimitivePropertyConfiguration primitiveProperty =
                Assert.IsType<PrimitivePropertyConfiguration>(propertyConfiguration);
            Assert.False(primitiveProperty.OptionalProperty);
        }

        [Fact]
        public void CanAddPrimitiveProperty_ForDependentEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            builder.Entity<Role>().HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.UserId);

            // Assert
            EntityTypeConfiguration dependentEntityType =
                builder.StructuralTypes.OfType<EntityTypeConfiguration>().FirstOrDefault(e => e.Name == "Role");
            Assert.NotNull(dependentEntityType);

            PrimitivePropertyConfiguration primitiveConfig =
                Assert.Single(dependentEntityType.Properties.OfType<PrimitivePropertyConfiguration>());
            Assert.Equal("UserForeignKey", primitiveConfig.Name);
            Assert.Equal("System.Int32", primitiveConfig.RelatedClrType.FullName);
            Assert.False(primitiveConfig.OptionalProperty);
        }

        [Fact]
        public void HasOptional_CanSetMultipleForeignKeyProperties_ForReferencialConstraint()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.Entity<MultiRole>()
                    .HasOptional(r => r.User, (r, u) => r.UserKey1 == u.UserId1 && r.UserKey2 == u.UserId2);

            // Assert
            Assert.Equal(2, navigationProperty.DependentProperties.Count());
            Assert.Same(typeof(MultiRole).GetProperty("UserKey1"), navigationProperty.DependentProperties.First());
            Assert.Same(typeof(MultiRole).GetProperty("UserKey2"), navigationProperty.DependentProperties.Last());

            Assert.Equal(2, navigationProperty.PrincipalProperties.Count());
            Assert.Same(typeof(MultiUser).GetProperty("UserId1"), navigationProperty.PrincipalProperties.First());
            Assert.Same(typeof(MultiUser).GetProperty("UserId2"), navigationProperty.PrincipalProperties.Last());
        }

        [Fact]
        public void HasRequired_CanSetMultipleForeignKeyProperties_ForReferencialConstraint()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.Entity<MultiRole>()
                    .HasRequired(r => r.User, (r, u) => r.UserKey1 == u.UserId1 && r.UserKey2 == u.UserId2);

            // Assert
            Assert.Equal(2, navigationProperty.DependentProperties.Count());
            Assert.Same(typeof(MultiRole).GetProperty("UserKey1"), navigationProperty.DependentProperties.First());
            Assert.Same(typeof(MultiRole).GetProperty("UserKey2"), navigationProperty.DependentProperties.Last());

            Assert.Equal(2, navigationProperty.PrincipalProperties.Count());
            Assert.Same(typeof(MultiUser).GetProperty("UserId1"), navigationProperty.PrincipalProperties.First());
            Assert.Same(typeof(MultiUser).GetProperty("UserId2"), navigationProperty.PrincipalProperties.Last());
        }

        [Fact]
        public void GetEdmModel_Works_WithSingleForeignKeyProperty()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<User> userType = builder.Entity<User>();
            userType.HasKey(u => u.UserId).HasMany(u => u.Roles);

            EntityTypeConfiguration<Role> roleType = builder.Entity<Role>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.UserId)
                .CascadeOnDelete();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(Role));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(User), false, EdmMultiplicity.One);

            Assert.Equal(EdmOnDeleteAction.Cascade, usersNav.OnDelete);

            IEdmStructuralProperty dependentProperty = Assert.Single(usersNav.DependentProperties);
            Assert.Equal("UserForeignKey", dependentProperty.Name);

            IEdmProperty edmProperty = Assert.Single(roleEntityType.Properties().Where(c => c.Name == "UserForeignKey"));
            Assert.False(edmProperty.Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_Works_WithMultiForeignKeys()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<MultiUser> userType = builder.Entity<MultiUser>();
            userType.HasKey(u => new { u.UserId1, u.UserId2 })
                .HasMany(u => u.Roles);

            EntityTypeConfiguration<MultiRole> roleType = builder.Entity<MultiRole>();
            roleType.HasKey(r => r.RoleId)
                .HasOptional(r => r.User, (r, u) => r.UserKey1 == u.UserId1 && r.UserKey2 == u.UserId2)
                .CascadeOnDelete();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(MultiRole));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(MultiUser), true, EdmMultiplicity.ZeroOrOne);

            Assert.Equal(EdmOnDeleteAction.Cascade, usersNav.OnDelete);

            Assert.Equal(2, usersNav.DependentProperties.Count());
            Assert.Equal("UserKey1", usersNav.DependentProperties.First().Name);
            Assert.Equal("UserKey2", usersNav.DependentProperties.Last().Name);

            IEdmProperty edmProperty = Assert.Single(roleEntityType.Properties().Where(c => c.Name == "UserKey1"));
            Assert.False(edmProperty.Type.IsNullable);

            edmProperty = Assert.Single(roleEntityType.Properties().Where(c => c.Name == "UserKey2"));
            Assert.False(edmProperty.Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_ThrowsException_WithDifferentNumber()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            builder.Entity<MultiUser>().HasKey(u => u.UserId1);
            builder.Entity<MultiRole>()
                .HasRequired(r => r.User, (r, u) => r.UserKey1 == u.UserId1 && r.UserKey2 == u.UserId2);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
                String.Format(SRResources.DependentPropertiesNotMatchWithPrincipalKeys,
                "System.Int32,System.String", "System.Int32"));
        }

        [Fact]
        public void GetEdmModel_ThrowsException_WithDifferentTypes()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            builder.Entity<MultiUser>()
                .HasKey(u => new { u.UserId1, u.UserId2 });

            builder.Entity<MultiRole>()
                .HasRequired(r => r.User, (r, u) => r.UserKey2 == u.UserId2 && r.UserKey3 == u.UserId3);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => builder.GetEdmModel(),
                String.Format(SRResources.DependentPropertiesNotMatchWithPrincipalKeys,
                "System.String,System.Guid", "System.Int32,System.String"));
        }

        [Fact]
        public void SetNonPrimitiveProperty_ThrowsException()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => builder.Entity<ForeignEntity>().HasRequired(c => c.Principal,
                    (c, r) => c.InvalidForeignKey == r.InvalidPrincipalKey),
                String.Format(SRResources.ReferentialConstraintPropertyTypeNotValid, "System.Web.Http.OData.MockType"));
        }

        [Fact]
        public void GetEdmModel_PropertyWithConcurrency_IsConcurrencyToken()
        {
            // Arrange
            var builder = new ODataModelBuilder();
            builder.Entity<Customer>().Property(c => c.Name).IsConcurrencyToken();
            builder.Entity<Customer>().Property(c => c.Id);

            // Act
            var model = builder.GetEdmModel();

            // Assert
            IEdmEntityType type = model.AssertHasEntityType(typeof(Customer));
            IEdmStructuralProperty nameProperty =
                type.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, nameProperty.ConcurrencyMode);

            IEdmStructuralProperty idProperty =
                type.AssertHasPrimitiveProperty(model, "Id", EdmPrimitiveTypeKind.Int32, isNullable: false);
            Assert.Equal(EdmConcurrencyMode.None, idProperty.ConcurrencyMode);
        }

        class User
        {
            public int UserId { get; set; }
            public IList<Role> Roles { get; set; }
        }

        class Role
        {
            public int RoleId { get; set; }
            public int UserForeignKey { get; set; }
            public User User { get; set; }
        }

        class MultiUser
        {
            public int UserId1 { get; set; }
            public string UserId2 { get; set; }
            public Guid UserId3 { get; set; }
            public IList<MultiRole> Roles { get; set; }
        }

        class MultiRole
        {
            public int RoleId { get; set; }
            public int UserKey1 { get; set; }
            public string UserKey2 { get; set; }
            public Guid UserKey3 { get; set; }
            public MultiUser User { get; set; }
        }

        class ForeignPrincipal
        {
            public MockType InvalidPrincipalKey { get; set; }
        }

        private class ForeignEntity
        {
            public MockType InvalidForeignKey { get; set; }

            public ForeignPrincipal Principal { get; set; }
        }
    }
}
