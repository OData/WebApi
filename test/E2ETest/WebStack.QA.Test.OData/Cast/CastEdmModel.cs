using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Cast
{
    internal class CastEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Product> employees = builder.EntitySet<Product>("Products");
            var airPlaneType=builder.EntityType<AirPlane>();
            airPlaneType.DerivesFrom<Product>();

            builder.Namespace = typeof(Product).Namespace;

            var edmModel = builder.GetEdmModel();
            return edmModel;
        }
    }
}
