using System.Linq;

namespace System.Web.Http.Query
{
    public class QueryableController : ApiController
    {
        [Queryable]
        public IQueryable<Customer> Get()
        {
            return Enumerable.Range(0, 10)
                .Select(i => new Customer { ID = i, Name = "" + ('A' + i), ZipCode = 98000 - i })
                .AsQueryable();
        }

        public struct Customer
        {
            public int ID { get; set; }

            public string Name { get; set; }

            public int ZipCode { get; set; }
        }
    }
}
