using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using WebStack.QA.Test.OData.Common.Models.Products;

namespace WebStack.QA.Test.OData.QueryComposition.Controllers
{
    public class TopSkipOrderByTestsController : ApiController
    {
        [EnableQuery(PageSize = 999999)]
        public IEnumerable<Customer> GetByQuerableAttribute()
        {
            return new Customer[] 
            {
                new Customer() { Id = 1, Name = "Tom" },
                new Customer() { Id = 2, Name = "Jerry" },
                new Customer() { Id = 2, Name = "Mike" }
            };
        }

        public IEnumerable<Customer> GetByODataQueryOptions(ODataQueryOptions options)
        {
            var customers = this.GetByQuerableAttribute().AsQueryable();

            return options.ApplyTo(customers) as IQueryable<Customer>;
        }

        [EnableQuery(PageSize = 999999)]
        public HttpResponseMessage GetHttpResponseByQuerableAttribute()
        {
            return this.Request.CreateResponse<IEnumerable<Customer>>(System.Net.HttpStatusCode.OK, GetByQuerableAttribute());
        }

        [EnableQuery(PageSize = 999999)]
        public IEnumerable<Customer> GetCustomerCollection()
        {
            var col = new CustomerCollection();
            col.Add(new Customer() { Id = 1, Name = "Tom" });
            col.Add(new Customer() { Id = 2, Name = "Jerry" });
            return col;
        }

        public IEnumerable<ODataRawQueryOptions> GetODataQueryOptions(ODataQueryOptions options)
        {
            return new ODataRawQueryOptions[] { options.RawValues };
        }
    }
}
