using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Aggregation
{
    public class AggregationEdmModel
    {
        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
