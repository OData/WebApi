//-----------------------------------------------------------------------------
// <copyright file="CountEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Data.Entity;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ODataCountTest
{
    public class CountEdmModel
    {
        public class CountContext : DbContext
        {
            public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=CountTest1";

            public CountContext()
                : base(ConnectionString)
            {
            }

            public IDbSet<Hero> Heroes { get; set; }
            public IDbSet<Weapon> Weapons { get; set; }
        }

        public static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
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
