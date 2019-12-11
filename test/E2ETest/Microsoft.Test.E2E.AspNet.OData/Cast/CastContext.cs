// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class DataSource: IDisposable
    {
        private static ProductsContext _context = null;
        private static IQueryable<Product> _products = null;

        public static IQueryable<Product> InMemoryProducts
        {
            get
            {
                if (_products == null)
                {
                    _products = new List<Product>()
                    {
                        new Product()
                        {
                            ID=1,
                            Name="Name1",
                            Domain=Domain.Military,
                            Weight=1.1,
                            DimensionInCentimeter=new List<int>{1,2,3},
                            ManufacturingDate=new System.DateTimeOffset(2011,1,1,0,0,0,TimeSpan.FromHours(8)),
                        },
                        new Product()
                        {
                            ID=2,
                            Name="Name2",
                            Domain=Domain.Civil,
                            Weight=2.2,
                            DimensionInCentimeter=new List<int>{2,3,4},
                            ManufacturingDate=new System.DateTimeOffset(2012,1,1,0,0,0,TimeSpan.FromHours(8)),
                        },
                        new Product()
                        {
                            ID=3,
                            Name="Name3",
                            Domain=Domain.Both,
                             Weight=3.3,
                            DimensionInCentimeter=new List<int>{3,4,5},
                            ManufacturingDate=new System.DateTimeOffset(2013,1,1,0,0,0,TimeSpan.FromHours(8)),
                        },
                      
                   }.AsQueryable<Product>();
                }
                return _products;
            }
        }

        public static IQueryable<Product> EfProducts
        {
            get
            {
                if (_context == null)
                {
                    Database.SetInitializer(new DropCreateDatabaseAlways<ProductsContext>());
                    string databaseName = "CastTest_" + DateTime.Now.Ticks.ToString();
                    string connectionString = string.Format(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog={0}", databaseName);

                    _context = new ProductsContext(connectionString);
                    foreach (Product product in DataSource.InMemoryProducts)
                    {
                        _context.Products.Add(product);
                    }
                    _context.SaveChanges();
                }
                return _context.Products;
            }
        }

        public void Dispose()
        {
            if(_context != null) 
            {
                // _context.Dispose();
            }
        }
    }

    public class ProductsContext : DbContext
    {
        public ProductsContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
