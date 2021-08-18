//-----------------------------------------------------------------------------
// <copyright file="OrderByEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Data.Entity;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class OrderByEdmModel
    {
        public class OrderByContext : DbContext
        {
            private static readonly string ConnectionString =
                @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=OrderByColumnTest";

            public OrderByContext()
                : base(ConnectionString)
            {
            }

            public IDbSet<Item> Items { get; set; }
            public IDbSet<Item2> Items2 { get; set; }
            public IDbSet<ItemWithEnum> ItemsWithEnum { get; set; }
        }

        public static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataConventionModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<Item>("Items");
            builder.EntitySet<Item2>("Items2");
            builder.EntitySet<ItemWithEnum>("ItemsWithEnum");
            builder.EntitySet<ItemWithoutColumn>("ItemsWithoutColumn");
            builder.EnumType<SmallNumber>();
            return builder.GetEdmModel();
        }
    }
}
