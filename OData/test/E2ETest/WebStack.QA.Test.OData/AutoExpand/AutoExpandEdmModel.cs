using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.AutoExpand
{
    public class AutoExpandEdmModel
    {
        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<ChoiceOrder>("OrderChoices");
            builder.EntitySet<People>("People");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
