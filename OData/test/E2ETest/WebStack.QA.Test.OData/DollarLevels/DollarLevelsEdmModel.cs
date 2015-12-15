using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.DollarLevels
{
    public class DollarLevelsEdmModel
    {
        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DLManager>("DLManagers");
            builder.EntitySet<DLEmployee>("DLEmployees");

            builder.Namespace = typeof(DLManager).Namespace;
            return builder.GetEdmModel();
        }
    }
}
