//-----------------------------------------------------------------------------
// <copyright file="BindableOperationFinderAnnotationTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Builder
{
    public class BindableOperationFinderAnnotationTest
    {
        [Fact]
        public void CanBuildBoundOperationCacheForIEdmModel()
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
            BindableOperationFinder annotation = new BindableOperationFinder(model);
            IEdmAction[] movieActions = annotation.FindOperations(movieType)
                .OfType<IEdmAction>()
                .ToArray();
            IEdmAction[] customerActions = annotation.FindOperations(customerType)
                .OfType<IEdmAction>()
                .ToArray();
            IEdmAction[] blockBusterActions = annotation.FindOperations(blockBusterType)
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

        [Fact]
        public void CanBuildOperationBoundToCollectionCacheForIEdmModel()
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

            // build actions that are bindable to the collection of entity
            customer.Collection.Action("CollectionCustomerActionInCache1");
            customer.Collection.Action("CollectionCustomerActionInCache2");
            movie.Collection.Action("CollectionMovieActionInCache3");

            ActionConfiguration movieActionIncache4 = builder.Action("CollectionMovieActionInCache4");
            CollectionTypeConfiguration collectionConfiguration = new CollectionTypeConfiguration(movieConfiguration, typeof(Movie));
            movieActionIncache4.SetBindingParameter("bindingParameter", collectionConfiguration);

            blockBuster.Collection.Action("CollectionBlockbusterActionInCache5");

            // build functions that are bindable to the collection of entity
            customer.Collection.Function("CollectionCustomerFunctionInCache1").Returns<int>();
            customer.Collection.Function("CollectionCustomerFunctionInCache2").Returns<int>();
            movie.Collection.Function("CollectionMovieFunctionInCache3").Returns<int>();
            blockBuster.Collection.Function("CollectionBlockbusterFunctionInCache5").Returns<int>();

            // build actions that are either: bindable to an entity, have no parameter, have only complex parameter 
            customer.Action("CustomersActionNotInCache1");
            customer.Function("CustomersFunctionNotInCache1").Returns<int>();
            movie.Action("MoviesActionNotInCache2");
            builder.Action("NoParametersNotInCache3");

            ActionConfiguration addressParameterNotInCache4 = builder.Action("AddressParameterNotInCache4");
            addressParameterNotInCache4.Parameter<Address>("address");

            IEdmModel model = builder.GetEdmModel();

            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Customer");
            IEdmEntityType movieType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Movie");
            IEdmEntityType blockBusterType = model.SchemaElements.OfType<IEdmEntityType>().Single(e => e.Name == "Blockbuster");

            // Act
            BindableOperationFinder annotation = new BindableOperationFinder(model);
            var movieOperations = annotation.FindOperationsBoundToCollection(movieType).ToArray();
            var customerOperations = annotation.FindOperationsBoundToCollection(customerType).ToArray();
            var blockBusterOperations = annotation.FindOperationsBoundToCollection(blockBusterType).ToArray();

            // Assert
            Assert.Equal(3, movieOperations.Length);
            Assert.Single(movieOperations.Where(a => a.Name == "CollectionMovieActionInCache3"));
            Assert.Single(movieOperations.Where(a => a.Name == "CollectionMovieActionInCache4"));
            Assert.Single(movieOperations.Where(a => a.Name == "CollectionMovieFunctionInCache3"));

            Assert.Equal(4, customerOperations.Length);
            Assert.Single(customerOperations.Where(a => a.Name == "CollectionCustomerActionInCache1"));
            Assert.Single(customerOperations.Where(a => a.Name == "CollectionCustomerActionInCache2"));
            Assert.Single(customerOperations.Where(a => a.Name == "CollectionCustomerFunctionInCache1"));
            Assert.Single(customerOperations.Where(a => a.Name == "CollectionCustomerFunctionInCache2"));
            
            Assert.Equal(5, blockBusterOperations.Length);
            Assert.Single(blockBusterOperations.Where(a => a.Name == "CollectionBlockbusterActionInCache5"));
            Assert.Single(blockBusterOperations.Where(a => a.Name == "CollectionBlockbusterFunctionInCache5"));
            Assert.Single(blockBusterOperations.Where(a => a.Name == "CollectionMovieActionInCache3"));
            Assert.Single(blockBusterOperations.Where(a => a.Name == "CollectionMovieActionInCache4"));
            Assert.Single(blockBusterOperations.Where(a => a.Name == "CollectionMovieFunctionInCache3"));
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
