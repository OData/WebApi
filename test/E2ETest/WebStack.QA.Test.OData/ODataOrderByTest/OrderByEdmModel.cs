﻿using System.Data.Entity;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    public  class OrderByEdmModel
    {
        public class OrderByContext: DbContext
        {
            private static readonly string ConnectionString =
                @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=OrderByTest";

            public OrderByContext()
                : base(ConnectionString)
            {
            }

            public IDbSet<Item> Items { get; set; }
        }
        

        public static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Item>("Items");

            return builder.GetEdmModel();
        }

    }
}