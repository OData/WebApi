// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class BindableProcedureFinderAnnotationTest
    {
        [Fact]
        public void CanBuildBoundProcedureCacheForIEdmModel()
        {
            // Arrange            
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            customer.HasKey(c => c.ID);
            customer.Property(c => c.Name);
            customer.ComplexProperty(c => c.Address);

            EntityTypeConfiguration<Movie> movie = builder.EntitySet<Movie>("Movies").EntityType;
            movie.HasKey(m => m.ID);
            movie.HasKey(m => m.Name);
            EntityTypeConfiguration<Blockbuster> blockBuster = builder.EntityType<Blockbuster>().DerivesFrom<Movie>();
            EntityTypeConfiguration movieConfiguration = builder.StructuralTypes.OfType<EntityTypeConfiguration>().Single(t => t.Name == "Movie");

            // build actions that are bindable to a single entity
            customer.Action("InCache1_CustomerAction");
            customer.Action("InCache2_CustomerAction");
            movie.Action("InCache3_MovieAction");
            ActionConfiguration incache4_MovieAction = builder.Action("InCache4_MovieAction");
            incache4_MovieAction.SetBindingParameter("bindingParameter", movieConfiguration);
            blockBuster.Action("InCache5_BlockbusterAction");

            // build actions that are either: bindable to a collection of entities, have no parameter, have only complex parameter 
            customer.Collection.Action("NotInCache1_CustomersAction");
            movie.Collection.Action("NotInCache2_MoviesAction");
            ActionConfiguration notInCache3_NoParameters = builder.Action("NotInCache3_NoParameters");
            ActionConfiguration notInCache4_AddressParameter = builder.Action("NotInCache4_AddressParameter");
            notInCache4_AddressParameter.Parameter<Address>("address");

            IEdmModel model = builder.GetEdmModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Customer");
            IEdmEntityType movieType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Movie");
            IEdmEntityType blockBusterType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Blockbuster");

            // Act 
            BindableProcedureFinder annotation = new BindableProcedureFinder(model);
            IEdmAction[] movieActions = annotation.FindProcedures(movieType)
                .OfType<IEdmAction>()
                .ToArray();
            IEdmAction[] customerActions = annotation.FindProcedures(customerType)
                .OfType<IEdmAction>()
                .ToArray();
            IEdmAction[] blockBusterActions = annotation.FindProcedures(blockBusterType)
                .OfType<IEdmAction>()
                .ToArray();

            // Assert
            Assert.Equal(2, customerActions.Length);
            Assert.NotNull(customerActions.SingleOrDefault(a => a.Name == "InCache1_CustomerAction"));
            Assert.NotNull(customerActions.SingleOrDefault(a => a.Name == "InCache2_CustomerAction"));
            Assert.Equal(2, movieActions.Length);
            Assert.NotNull(movieActions.SingleOrDefault(a => a.Name == "InCache3_MovieAction"));
            Assert.NotNull(movieActions.SingleOrDefault(a => a.Name == "InCache4_MovieAction"));
            Assert.Equal(3, blockBusterActions.Length);
            Assert.NotNull(blockBusterActions.SingleOrDefault(a => a.Name == "InCache3_MovieAction"));
            Assert.NotNull(blockBusterActions.SingleOrDefault(a => a.Name == "InCache4_MovieAction"));
            Assert.NotNull(blockBusterActions.SingleOrDefault(a => a.Name == "InCache5_BlockbusterAction"));
        }

        public class Movie
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class Blockbuster : Movie
        {
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int ZipCode { get; set; }
        }
    }
}
