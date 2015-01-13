// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.OData.Properties;
using System.Web.OData.TestCommon;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;
using BuilderTestModels = System.Web.OData.Builder.TestModels;

namespace System.Web.OData.Builder
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
            builder.AddEntityType(typeof(Customer));

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

            Assert.Reflection.Property(builder, b => b.DataServiceVersion, new Version(4, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void MaxDataServiceVersion_RoundTrips()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            Assert.Reflection.Property(builder, b => b.MaxDataServiceVersion, new Version(4, 0), allowNull: false, roundTripTestValue: new Version(1, 0));
        }

        [Fact]
        public void EntityContainer_Is_Default()
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            IEdmModel model = builder.GetEdmModel();

            Assert.Same(model.EntityContainer, model.SchemaElements.OfType<IEdmEntityContainer>().Single());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ActionLink_PreservesFollowsConventions(bool value)
        {
            // Arrange
            ODataModelBuilder builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            ActionConfiguration configuration = new ActionConfiguration(builder, "IgnoreAction");
            Mock<IEdmTypeConfiguration> bindingParameterTypeMock = new Mock<IEdmTypeConfiguration>();
            bindingParameterTypeMock.Setup(o => o.Kind).Returns(EdmTypeKind.Entity);
            Type entityType = typeof(object);
            bindingParameterTypeMock.Setup(o => o.ClrType).Returns(entityType);
            configuration.SetBindingParameter("IgnoreParameter", bindingParameterTypeMock.Object);
            configuration.HasActionLink((a) => { throw new NotImplementedException(); }, followsConventions: value);
            builder.AddProcedure(configuration);
            builder.AddEntityType(entityType);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            var action = Assert.Single(model.SchemaElements.OfType<IEdmAction>());
            ActionLinkBuilder actionLinkBuilder = model.GetActionLinkBuilder(action);
            Assert.NotNull(actionLinkBuilder);
            Assert.Equal(value, actionLinkBuilder.FollowsConventions);
        }

        [Fact]
        public void GetEdmModel_PropertyWithETag_IsConcurrencyToken()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            customer.Property(c => c.Name).IsConcurrencyToken();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            IEdmEntityType type = model.AssertHasEntityType(typeof(Customer));
            IEdmStructuralProperty property =
                type.AssertHasPrimitiveProperty(model, "Name", EdmPrimitiveTypeKind.String, isNullable: true);
            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode);
        }

        [Fact]
        public void GetEdmModel_DoesntCreateOperationImport_For_BoundedOperations()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntitySetConfiguration<Customer> customers = builder.EntitySet<Customer>("Customers");
            customers.EntityType.HasKey(c => c.Id);
            customers.EntityType.Action("Action").Returns<bool>();
            customers.EntityType.Collection.Action("CollectionAction").Returns<bool>();
            customers.EntityType.Function("Function").Returns<bool>();
            customers.EntityType.Collection.Function("Function").Returns<bool>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(0, model.EntityContainer.OperationImports().Count());
        }

        [Fact]
        public void GetEdmModel_CreatesOperationImports_For_UnboundedOperations()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.Function("Function").Returns<bool>();
            builder.Action("Action").Returns<bool>();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.Equal(2, model.EntityContainer.OperationImports().Count());
        }

        [Fact]
        public void GetEdmModel_HasContainment_WithModelBuilder()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var myOrder = builder.EntityType<BuilderTestModels.MyOrder>();
            myOrder.ContainsMany(mo => mo.OrderLines);
            myOrder.ContainsRequired(mo => mo.OrderHeader);
            myOrder.ContainsOptional(mo => mo.OrderCancellation);
            builder.EntitySet<BuilderTestModels.MyOrder>("MyOrders");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MyOrders");
            Assert.NotNull(myOrders);
            var edmMyOrder = myOrders.EntityType();
            Assert.Equal("MyOrder", edmMyOrder.Name);
            AssertHasContainment(edmMyOrder, model);
        }

        [Fact]
        public void GetEdmModel_HasContainment_WithLowLevelModelBuilder()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var myOrder = builder.AddEntityType(typeof(BuilderTestModels.MyOrder));
            myOrder.HasKey(typeof(BuilderTestModels.MyOrder).GetProperty("ID"));
            myOrder.AddContainedNavigationProperty(typeof(BuilderTestModels.MyOrder).GetProperty("OrderLines"), EdmMultiplicity.Many);
            myOrder.AddContainedNavigationProperty(typeof(BuilderTestModels.MyOrder).GetProperty("OrderHeader"), EdmMultiplicity.One);
            myOrder.AddContainedNavigationProperty(
                typeof(BuilderTestModels.MyOrder).GetProperty("OrderCancellation"),
                EdmMultiplicity.ZeroOrOne);
            builder.AddEntitySet("MyOrders", myOrder);

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MyOrders");
            Assert.NotNull(myOrders);
            var edmMyOrder = myOrders.EntityType();
            Assert.Equal("MyOrder", edmMyOrder.Name);
            AssertHasContainment(edmMyOrder, model);
        }

        [Fact]
        public void GetEdmModel_DerivedTypeHasContainment_WithModelBuilder()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var myOrder = builder.EntityType<BuilderTestModels.MyOrder>();
            myOrder.ContainsMany(mo => mo.OrderLines);
            myOrder.ContainsRequired(mo => mo.OrderHeader);
            myOrder.ContainsOptional(mo => mo.OrderCancellation);
            var mySpecialOrder = builder.EntityType<BuilderTestModels.MySpecialOrder>().DerivesFrom<BuilderTestModels.MyOrder>();
            mySpecialOrder.ContainsOptional(order => order.Gift);
            builder.EntitySet<BuilderTestModels.MySpecialOrder>("MySpecialOrders");

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MySpecialOrders");
            Assert.NotNull(myOrders);
            var edmMyOrder = myOrders.EntityType();
            Assert.Equal("MySpecialOrder", edmMyOrder.Name);
            AssertHasContainment(edmMyOrder, model);
            AssertHasAdditionalContainment(edmMyOrder, model);
        }

        [Fact]
        public void GetEdmModel_DerivedTypeHasContainment_WithLowLevelModelBuilder()
        {
            // Arrange
            var builder = ODataModelBuilderMocks.GetModelBuilderMock<ODataModelBuilder>();
            var myOrder = builder.AddEntityType(typeof(BuilderTestModels.MyOrder));
            myOrder.HasKey(typeof(BuilderTestModels.MyOrder).GetProperty("ID"));
            myOrder.AddContainedNavigationProperty(typeof(BuilderTestModels.MyOrder).GetProperty("OrderLines"), EdmMultiplicity.Many);
            myOrder.AddContainedNavigationProperty(typeof(BuilderTestModels.MyOrder).GetProperty("OrderHeader"), EdmMultiplicity.One);
            myOrder.AddContainedNavigationProperty(
                typeof(BuilderTestModels.MyOrder).GetProperty("OrderCancellation"),
                EdmMultiplicity.ZeroOrOne);
            var mySpecialOrder = builder.AddEntityType(typeof(BuilderTestModels.MySpecialOrder));
            mySpecialOrder.DerivesFrom(myOrder);
            mySpecialOrder.AddContainedNavigationProperty(
                typeof(BuilderTestModels.MySpecialOrder).GetProperty("Gift"),
                EdmMultiplicity.ZeroOrOne);
            builder.AddEntitySet("MySpecialOrders", mySpecialOrder);

            // Act & Assert
            IEdmModel model = builder.GetEdmModel();
            Assert.NotNull(model);

            var container = Assert.Single(model.SchemaElements.OfType<IEdmEntityContainer>());

            var myOrders = container.FindEntitySet("MySpecialOrders");
            Assert.NotNull(myOrders);
            var edmMyOrder = myOrders.EntityType();
            Assert.Equal("MySpecialOrder", edmMyOrder.Name);
            AssertHasContainment(edmMyOrder, model);
            AssertHasAdditionalContainment(edmMyOrder, model);
        }

        internal static void AssertHasContainment(IEdmEntityType myOrder, IEdmModel model)
        {
            var orderLines = myOrder.AssertHasNavigationProperty(
                model,
                "OrderLines",
                typeof(BuilderTestModels.OrderLine),
                isNullable: false,
                multiplicity: EdmMultiplicity.Many);
            Assert.True(orderLines.ContainsTarget);

            var orderHeader = myOrder.AssertHasNavigationProperty(
                model,
                "OrderHeader",
                typeof(BuilderTestModels.OrderHeader),
                isNullable: false,
                multiplicity: EdmMultiplicity.One);
            Assert.True(orderHeader.ContainsTarget);

            var orderCancellation = myOrder.AssertHasNavigationProperty(
                model,
                "OrderCancellation",
                typeof(BuilderTestModels.OrderCancellation),
                isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);
            Assert.True(orderCancellation.ContainsTarget);
        }

        internal static void AssertHasAdditionalContainment(IEdmEntityType mySpecialOrder, IEdmModel model)
        {
            var gift = mySpecialOrder.AssertHasNavigationProperty(
                model,
                "Gift",
                typeof(BuilderTestModels.Gift),
                isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);
            Assert.True(gift.ContainsTarget);
        }

        [Fact]
        public void Validate_Throws_If_Entity_Doesnt_Have_Key_Defined()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<Customer>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.GetEdmModel(), "The entity 'Customer' does not have a key defined.");
        }

        [Fact]
        public void Validate_Doesnt_Throw_For_Derived_Entities()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            builder.EntityType<BuilderTestModels.Car>().DerivesFrom<BuilderTestModels.Vehicle>();
            builder.EntityType<BuilderTestModels.Vehicle>().HasKey(v => v.Name);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void GetEdmModel_CanSetReferentialConstraint_WithKeyPrincipal()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<User> userType = builder.EntityType<User>();
            userType.HasKey(u => u.UserId).HasMany(u => u.Roles);

            EntityTypeConfiguration<Role> roleType = builder.EntityType<Role>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.UserId)
                .CascadeOnDelete();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(Role));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(User), isNullable: false, multiplicity: EdmMultiplicity.One);

            Assert.Equal(EdmOnDeleteAction.Cascade, usersNav.OnDelete);

            IEdmStructuralProperty dependentProperty = Assert.Single(usersNav.DependentProperties());
            Assert.Equal("UserForeignKey", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(usersNav.PrincipalProperties());
            Assert.Equal("UserId", principalProperty.Name);
        }

        [Fact]
        public void GetEdmModel_CanSetReferentialConstraint_WithCustomPrincipal()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<User> userType = builder.EntityType<User>();
            userType.HasKey(c => c.UserId).HasMany(c => c.Roles);

            EntityTypeConfiguration<Role> roleType = builder.EntityType<Role>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.NotKeyPrincipal)
                .CascadeOnDelete(false);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(Role));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(User), isNullable: false, multiplicity: EdmMultiplicity.One);

            Assert.Equal(EdmOnDeleteAction.None, usersNav.OnDelete);

            IEdmStructuralProperty dependentProperty = Assert.Single(usersNav.DependentProperties());
            Assert.Equal("UserForeignKey", dependentProperty.Name);

            IEdmStructuralProperty principalProperty = Assert.Single(usersNav.PrincipalProperties());
            Assert.Equal("NotKeyPrincipal", principalProperty.Name);
        }

        [Fact]
        public void GetEdmModel_CanSetDependentPropertyNonNullable_ForRequiredNavigation()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<User> userType = builder.EntityType<User>();
            userType.HasKey(c => c.UserId).HasMany(c => c.Roles);

            EntityTypeConfiguration<Role> roleType = builder.EntityType<Role>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserForeignKey == u.UserId);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(Role));

            roleEntityType.AssertHasNavigationProperty(model, "User", typeof(User), isNullable: false,
                multiplicity: EdmMultiplicity.One);

            IEdmProperty edmProperty = Assert.Single(roleEntityType.Properties().Where(c => c.Name == "UserForeignKey"));
            Assert.False(edmProperty.Type.IsNullable);
        }

        [Fact]
        public void GetEdmModel_CanSetDependentPropertyNullable_ForOptionalNavigation()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<User> userType = builder.EntityType<User>();
            userType.HasKey(c => c.UserId).HasMany(c => c.Roles);
            userType.Property(u => u.StringPrincipal);

            EntityTypeConfiguration<Role> roleType = builder.EntityType<Role>();
            roleType.HasKey(r => r.RoleId)
                .HasOptional(r => r.User, (r, u) => r.UserStringForeignKey == u.StringPrincipal);

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(Role));

            roleEntityType.AssertHasNavigationProperty(model, "User", typeof(User), isNullable: true,
                multiplicity: EdmMultiplicity.ZeroOrOne);

            IEdmProperty edmProperty = Assert.Single(roleEntityType.Properties().Where(c => c.Name == "UserStringForeignKey"));
            Assert.True(edmProperty.Type.IsNullable);
        }

        class User
        {
            public int UserId { get; set; }

            public int NotKeyPrincipal { get; set; }

            public string StringPrincipal { get; set; }

            public IList<Role> Roles { get; set; }
        }

        class Role
        {
            public int RoleId { get; set; }

            public int UserForeignKey { get; set; }

            public string UserStringForeignKey { get; set; }

            public User User { get; set; }
        }

        [Fact]
        public void GetEdmModel_CanSetMultiReferentialConstraint_WithKeyPrincipal()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<MultiUser> userType = builder.EntityType<MultiUser>();
            userType.HasKey(c => c.UserId1)
                .HasKey(c => c.UserId2)
                .HasMany(c => c.Roles);

            EntityTypeConfiguration<MultiRole> roleType = builder.EntityType<MultiRole>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserKey1 == u.UserId1 && r.UserKey2 == u.UserId2)
                .CascadeOnDelete();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(MultiRole));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(MultiUser), isNullable: false, multiplicity: EdmMultiplicity.One);

            Assert.Equal(EdmOnDeleteAction.Cascade, usersNav.OnDelete);

            Assert.Equal(2, usersNav.DependentProperties().Count());
            Assert.Equal("UserKey1", usersNav.DependentProperties().First().Name);
            Assert.Equal("UserKey2", usersNav.DependentProperties().Last().Name);

            Assert.Equal(2, usersNav.PrincipalProperties().Count());
            Assert.Equal("UserId1", usersNav.PrincipalProperties().First().Name);
            Assert.Equal("UserId2", usersNav.PrincipalProperties().Last().Name);
        }

        [Fact]
        public void GetEdmModel_CanSetMultiReferentialConstraint_WithCustomPrincipal()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<MultiUser> userType = builder.EntityType<MultiUser>();
            userType.HasKey(c => c.UserId1)
                .HasKey(c => c.UserId2)
                .HasMany(c => c.Roles);

            EntityTypeConfiguration<MultiRole> roleType = builder.EntityType<MultiRole>();
            roleType.HasKey(r => r.RoleId)
                .HasRequired(r => r.User, (r, u) => r.UserKey1 == u.PrincipalUserKey1 && r.UserKey2 == u.PrincipalUserKey2)
                .CascadeOnDelete();

            // Act
            IEdmModel model = builder.GetEdmModel();

            // Assert
            Assert.NotNull(model);
            IEdmEntityType roleEntityType = model.AssertHasEntityType(typeof(MultiRole));

            IEdmNavigationProperty usersNav = roleEntityType.AssertHasNavigationProperty(model, "User",
                typeof(MultiUser), isNullable: false, multiplicity: EdmMultiplicity.One);

            Assert.Equal(EdmOnDeleteAction.Cascade, usersNav.OnDelete);

            Assert.Equal(2, usersNav.DependentProperties().Count());
            Assert.Equal("UserKey1", usersNav.DependentProperties().First().Name);
            Assert.Equal("UserKey2", usersNav.DependentProperties().Last().Name);

            Assert.Equal(2, usersNav.PrincipalProperties().Count());
            Assert.Equal("PrincipalUserKey1", usersNav.PrincipalProperties().First().Name);
            Assert.Equal("PrincipalUserKey2", usersNav.PrincipalProperties().Last().Name);
        }

        [Fact]
        public void GetEdmModel_ThrowsException_WithNotEqualExpression()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<MultiRole> roleType = builder.EntityType<MultiRole>();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => roleType.HasRequired(c => c.User,
                (c, r) => c.UserKey1 == r.PrincipalUserKey1 && r.PrincipalUserKey3),
                "Unsupported Expression NodeType 'MemberAccess'.");
        }

        class MultiUser
        {
            public int UserId1 { get; set; }

            public string UserId2 { get; set; }

            public int PrincipalUserKey1 { get; set; }

            public string PrincipalUserKey2 { get; set; }

            public bool PrincipalUserKey3 { get; set; }

            public IList<MultiRole> Roles { get; set; }
        }

        class MultiRole
        {
            public int RoleId { get; set; }

            public int UserKey1 { get; set; }

            public string UserKey2 { get; set; }

            public MultiUser User { get; set; }
        }

        [Fact]
        public void CanSetSinglePrincipalProperty_ForReferencialConstraint()
        {
            // Arrange
            PropertyInfo expectDependentPropertyInfo = typeof(ForeignEntity).GetProperty("ForeignKey1");
            PropertyInfo expectPrincipalPropertyInfo = typeof(ForeignPrincipal).GetProperty("PrincipalKey1");
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.EntityType<ForeignEntity>().HasRequired(c => c.Principal, (c, r) => c.ForeignKey1 == r.PrincipalKey1);

            // Assert
            PropertyInfo actualPropertyInfo = Assert.Single(navigationProperty.DependentProperties);
            Assert.Same(expectDependentPropertyInfo, actualPropertyInfo);

            actualPropertyInfo = Assert.Single(navigationProperty.PrincipalProperties);
            Assert.Same(expectPrincipalPropertyInfo, actualPropertyInfo);
        }

        [Fact]
        public void CanSetMultipleProperties_ForReferencialConstraint()
        {
            // Arrange
            PropertyInfo expectDependentPropertyInfo1 = typeof(ForeignEntity).GetProperty("ForeignKey1");
            PropertyInfo expectDependentPropertyInfo2 = typeof(ForeignEntity).GetProperty("ForeignKey2");

            PropertyInfo expectPrincipalPropertyInfo1 = typeof(ForeignPrincipal).GetProperty("PrincipalKey1");
            PropertyInfo expectPrincipalPropertyInfo2 = typeof(ForeignPrincipal).GetProperty("PrincipalKey2");

            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            NavigationPropertyConfiguration navigationProperty =
                builder.EntityType<ForeignEntity>()
                    .HasRequired(c => c.Principal, (c, r) => c.ForeignKey1 == r.PrincipalKey1 & c.ForeignKey2 == r.PrincipalKey2);

            // Assert
            Assert.Equal(2, navigationProperty.DependentProperties.Count());
            Assert.Same(expectDependentPropertyInfo1, navigationProperty.DependentProperties.First());
            Assert.Same(expectDependentPropertyInfo2, navigationProperty.DependentProperties.Last());

            Assert.Equal(2, navigationProperty.PrincipalProperties.Count());
            Assert.Same(expectPrincipalPropertyInfo1, navigationProperty.PrincipalProperties.First());
            Assert.Same(expectPrincipalPropertyInfo2, navigationProperty.PrincipalProperties.Last());
        }

        [Fact]
        public void CanAddPrimitiveProperty_ForDependentEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            builder.EntityType<ForeignEntity>().HasRequired(c => c.Principal, (c, r) => c.ForeignKey1 == r.PrincipalKey1);

            // Assert
            EntityTypeConfiguration dependentEntityType =
                builder.StructuralTypes.OfType<EntityTypeConfiguration>().FirstOrDefault(e => e.Name == "ForeignEntity");
            Assert.NotNull(dependentEntityType);

            PrimitivePropertyConfiguration primitiveConfig =
                Assert.Single(dependentEntityType.Properties.OfType<PrimitivePropertyConfiguration>());
            Assert.Equal("ForeignKey1", primitiveConfig.Name);
            Assert.Equal("System.Int32", primitiveConfig.RelatedClrType.FullName);
        }

        [Fact]
        public void CanAddPrimitiveProperty_ForTargetEntityType()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act
            builder.EntityType<ForeignEntity>().HasRequired(c => c.Principal, (c, r) => c.ForeignKey1 == r.PrincipalKey1);

            // Assert
            EntityTypeConfiguration principalEntityType = Assert.Single(
                builder.StructuralTypes.OfType<EntityTypeConfiguration>().Where(e => e.Name == "ForeignPrincipal"));

            PropertyConfiguration propertyConfig = Assert.Single(principalEntityType.Properties);
            PrimitivePropertyConfiguration primitiveConfig =
                Assert.IsType<PrimitivePropertyConfiguration>(propertyConfig);

            Assert.Equal("PrincipalKey1", primitiveConfig.Name);
            Assert.Equal("System.Int32", primitiveConfig.RelatedClrType.FullName);
        }

        [Fact]
        public void SetNonPrimitiveProperty_ThrowsException()
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => builder.EntityType<ForeignEntity>().HasRequired(c => c.Principal,
                    (c, r) => c.InvalidForeignKey == r.InvalidPrincipalKey),
                String.Format(SRResources.ReferentialConstraintPropertyTypeNotValid, "System.Web.OData.MockType"));
        }

        class ForeignPrincipal
        {
            public int PrincipalKey1 { get; set; }

            public string PrincipalKey2 { get; set; }

            public MockType InvalidPrincipalKey { get; set; }
        }

        class ForeignEntity
        {
            public int ForeignKey1 { get; set; }

            public string ForeignKey2 { get; set; }

            public ForeignEntity ForeignKeySelf { get; set; }

            public MockType InvalidForeignKey { get; set; }

            public ForeignPrincipal Principal { get; set; }
        }
    }
}
