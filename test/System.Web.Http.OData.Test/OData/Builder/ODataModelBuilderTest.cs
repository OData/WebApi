// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

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
            ActionConfiguration action = new ActionConfiguration(builder, "Format");
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
            ActionConfiguration action = new ActionConfiguration(builder, "Format");
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

            ActionConfiguration action1 = new ActionConfiguration(builder, "Format");
            ActionConfiguration action2 = new ActionConfiguration(builder, "Format");
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
    }
}
