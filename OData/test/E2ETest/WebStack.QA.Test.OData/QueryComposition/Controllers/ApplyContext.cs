using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using WebStack.QA.Test.OData.Common.Models;
using WebStack.QA.Test.OData.Common.Models.Products;

namespace WebStack.QA.Test.OData.QueryComposition.Controllers
{
    public class DataSource
    {
        private static ProductsContext _context = null;
        private static IQueryable<Product> _products = null;

        public static IQueryable<Product> EfProducts
        {
            get
            {
                EnsureContext();
                return _context.Products;
            }
        }

        public static IQueryable<Order> EfOrders
        {
            get
            {
                EnsureContext();
                return _context.Orders;
            }
        }

        public static IQueryable<OrderLine> EfOrderLines
        {
            get
            {
                EnsureContext();
                return _context.OrderLines;
            }
        }


        private static void EnsureContext()
        {
            if (_context == null)
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ProductsContext>());
                string databaseName = "ApplyTest_" + DateTime.Now.Ticks.ToString();
                string connectionString = string.Format(@"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog={0}", databaseName);

                _context = new ProductsContext(connectionString);
                foreach (Order order in ModelHelper.CreateOrderData())
                {
                    _context.Orders.Add(order);
                    foreach(var orderLine in order.OrderLines)
                    {
                        _context.OrderLines.Add(orderLine);
                        _context.Products.Add(orderLine.Product);
                    }
                }
                _context.SaveChanges();
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

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
