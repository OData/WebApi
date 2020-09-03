using System.Linq;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Test.Formatter.Class
{
    public class CollectionSerializerCustomersController : TestODataController
    {
        public ITestActionResult Get(ODataQueryOptions<CollectionSerializerCustomer> options)
        {
            IQueryable<CollectionSerializerCustomer> customers = new[]
            {
                    new CollectionSerializerCustomer{ID = 1, Name = "Name 1"},
                    new CollectionSerializerCustomer{ID = 2, Name = "Name 2"},
                    new CollectionSerializerCustomer{ID = 3, Name = "Name 3"},
                }.AsQueryable();

            IQueryable<IEdmEntityObject> appliedCustomers = options.ApplyTo(customers) as IQueryable<IEdmEntityObject>;

            return Ok(appliedCustomers);
        }
    }
}

namespace Microsoft.AspNet.OData.Test.Formatter.Interface
{
    public class CollectionSerializerCustomersController : TestODataController
    {
        public ITestActionResult Get(IODataQueryOptions<CollectionSerializerCustomer> options)
        {
            IQueryable<CollectionSerializerCustomer> customers = new[]
            {
                    new CollectionSerializerCustomer{ID = 1, Name = "Name 1"},
                    new CollectionSerializerCustomer{ID = 2, Name = "Name 2"},
                    new CollectionSerializerCustomer{ID = 3, Name = "Name 3"},
                }.AsQueryable();

            IQueryable<IEdmEntityObject> appliedCustomers = options.ApplyTo(customers) as IQueryable<IEdmEntityObject>;

            return Ok(appliedCustomers);
        }
    }
}
