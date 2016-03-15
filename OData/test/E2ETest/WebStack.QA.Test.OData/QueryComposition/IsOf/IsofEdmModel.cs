using System;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.QueryComposition.IsOf
{
    public static class IsofEdmModel
    {
        private static IEdmModel _model;

        public static IEdmModel GetEdmModel()
        {
            if (_model != null)
            {
                return _model;
            }

            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<BillingCustomer>("BillingCustomers");
            builder.EntitySet<BillingDetail>("Billings");
            return _model = builder.GetEdmModel();
        }
    }
}
