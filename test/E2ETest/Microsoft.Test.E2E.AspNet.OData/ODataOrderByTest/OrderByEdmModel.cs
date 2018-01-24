// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Data.Entity;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

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
