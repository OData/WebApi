using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using WebStack.QA.Test.OData.Common.Models;
using WebStack.QA.Test.OData.Common.Models.Products;

namespace WebStack.QA.Test.OData.QueryComposition.Controllers
{
    public class FilterTestsController : ApiController
    {
        [EnableQuery(PageSize = 999999)]
        public IQueryable<Product> GetProducts()
        {
            return ModelHelper.CreateRandomProducts().AsQueryable();
        }

        [EnableQuery(PageSize = 999999, MaxAnyAllExpressionDepth = 3)]
        public IEnumerable<Movie> GetMovies()
        {
            return ModelHelper.CreateMovieData();
        }

        [EnableQuery(PageSize = 999999, MaxAnyAllExpressionDepth = 3)]
        public IEnumerable<Movie> GetMoviesBig()
        {
            return ModelHelper.CreateMovieBigData();
        }

        [EnableQuery(PageSize = 999999)]
        public HttpResponseMessage GetProductsHttpResponse()
        {
            return this.Request.CreateResponse<IEnumerable<Product>>(System.Net.HttpStatusCode.OK, GetProducts());
        }

        [EnableQuery(PageSize = 999999)]
        public Task<IEnumerable<Product>> GetAsyncProducts()
        {
            return Task.Factory.StartNew<IEnumerable<Product>>(() =>
                {
                    return GetProducts();
                });
        }

        public IEnumerable<Customer> GetProductsByOptions(ODataQueryOptions options)
        {
            var products = this.GetProducts().AsQueryable();

            return options.ApplyTo(products) as IQueryable<Customer>;
        }

        [EnableQuery(PageSize = 999999)]
        public IQueryable GetProductsAsAnonymousType()
        {
            var products = this.GetProducts().Select(p =>
                new
                {
                    ID = p.ID,
                    Name = p.Name,
                    ReleaseDate = p.ReleaseDate,
                    Price = p.Price,
                    Amount = p.Amount,
                    DiscontinuedDate = p.DiscontinuedDate,
                    Taxable = p.Taxable,
                    DateTimeOffset = p.DateTimeOffset,
                    @Supplier = p.Supplier
                }).AsQueryable();

            return products;
        }
    }
}
