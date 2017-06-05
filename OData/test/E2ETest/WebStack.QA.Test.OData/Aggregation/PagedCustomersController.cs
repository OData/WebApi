using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData;

namespace WebStack.QA.Test.OData.Aggregation.Paged
{
    public class CustomersController : BaseCustomersController
    {
        [EnableQuery(PageSize = 5)]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers;
        }
    }
}
