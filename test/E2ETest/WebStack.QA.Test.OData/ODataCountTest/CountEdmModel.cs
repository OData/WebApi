using System.Data.Entity;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ODataCountTest
{
    public class CountEdmModel
    {
        public class CountContext : DbContext
        {
            public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=CountTest";

            public CountContext()
                : base(ConnectionString)
            {
            }

            public IDbSet<Hero> Heroes { get; set; }
            public IDbSet<Weapon> Weapons { get; set; }
        }

        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Hero>("Heroes");
            builder.EntitySet<Weapon>("Weapons");

            BuildFunctions(builder);

            return builder.GetEdmModel();
        }

        private static void BuildFunctions(ODataModelBuilder builder)
        {
            FunctionConfiguration GetWeapons =
                builder.EntityType<Hero>()
                    .Collection.Function("GetWeapons")
                    .ReturnsCollectionFromEntitySet<Weapon>("Weapons");
            GetWeapons.IsComposable = true;

            FunctionConfiguration GetNames =
                builder.EntityType<Hero>()
                    .Collection.Function("GetNames")
                    .ReturnsCollection<string>();
            GetNames.IsComposable = true;
        }
    }
}
