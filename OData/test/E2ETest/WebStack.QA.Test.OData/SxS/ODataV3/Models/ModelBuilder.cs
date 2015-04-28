using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;

namespace WebStack.QA.Test.OData.SxS.ODataV3.Models
{
    public static class ModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Part>("Parts");
            return builder.GetEdmModel();
        }
    }
}