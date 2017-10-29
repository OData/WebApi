using System.Data.Entity;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    public class OrderByEdmModel
    {
        public class OrderByContext : DbContext
        {
            private static readonly string ConnectionString =
                @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=OrderTest";

            public OrderByContext()
                : base(ConnectionString)
            {
            }

            public IDbSet<Item> Items { get; set; }
            public IDbSet<Item2> Items2 { get; set; }

        }


        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Item>("Items");
            builder.EntitySet<Item2>("Items2");
         
            return builder.GetEdmModel();
        }
    }
}
