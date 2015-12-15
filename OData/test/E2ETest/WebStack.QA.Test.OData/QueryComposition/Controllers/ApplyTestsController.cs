using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using WebStack.QA.Test.OData.Cast;

namespace WebStack.QA.Test.OData.QueryComposition.Controllers
{
    public class ApplyTestsController : ApiController
    {
        [EnableQuery]
        public IQueryable Get()
        {
            return DataSource.EfProducts;
        }
    }
}
