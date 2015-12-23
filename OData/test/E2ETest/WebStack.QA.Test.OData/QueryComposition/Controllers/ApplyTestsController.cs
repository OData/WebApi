using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;


namespace WebStack.QA.Test.OData.QueryComposition.Controllers
{
    public class ApplyTestsController : ApiController
    {
        [EnableQuery]
        public IQueryable GetProducts()
        {
            return DataSource.EfProducts;
        }

        [EnableQuery]
        public IQueryable GetOrders()
        {
            return DataSource.EfOrders;
        }

        [EnableQuery]
        public IQueryable GetOrderLines()
        {
            return DataSource.EfOrderLines;
        }

    }
}
