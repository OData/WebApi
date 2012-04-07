// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Web.Http.Data.Test.Models;

namespace Microsoft.Web.Http.Data.Test
{
    public class CatalogController : DataController
    {
        private Product[] products;

        public CatalogController()
        {
            this.products = new Product[] { 
                new Product { ProductID = 1, ProductName = "Frish Gnarbles", UnitPrice = 12.33M, UnitsInStock = 55 },
                new Product { ProductID = 2, ProductName = "Crispy Snarfs", UnitPrice = 4.22M, UnitsInStock = 11 },
                new Product { ProductID = 1, ProductName = "Cheezy Snax", UnitPrice = 2.99M, UnitsInStock = 21 },
                new Product { ProductID = 1, ProductName = "Fruit Yummies", UnitPrice = 5.55M, UnitsInStock = 88 },
                new Product { ProductID = 1, ProductName = "Choco Wafers", UnitPrice = 1.87M, UnitsInStock = 109 },
                new Product { ProductID = 1, ProductName = "Fritter Flaps", UnitPrice = 2.45M, UnitsInStock = 444 },
                new Product { ProductID = 1, ProductName = "Yummy Bears", UnitPrice = 2.00M, UnitsInStock = 27 },
                new Product { ProductID = 1, ProductName = "Cheddar Gnomes", UnitPrice = 3.99M, UnitsInStock = 975 },
                new Product { ProductID = 1, ProductName = "Beefcicles", UnitPrice = 0.99M, UnitsInStock = 634 },
                new Product { ProductID = 1, ProductName = "Butterscotchies", UnitPrice = 1.00M, UnitsInStock = 789 }
            };
        }

        [Queryable(ResultLimit = 9)]
        public IQueryable<Product> GetProducts()
        {
            return this.products.AsQueryable();
        }

        [Queryable]
        public IQueryable<Order> GetOrders()
        {
            return new Order[] { 
                new Order { OrderID = 1, CustomerID = "ALFKI" },
                new Order { OrderID = 2, CustomerID = "CHOPS" }
            }.AsQueryable();
        }

        public IEnumerable<Order_Detail> GetDetails(int orderId)
        {
            return Enumerable.Empty<Order_Detail>();
        }

        public void InsertOrder(Order order)
        {

        }

        public void UpdateProduct(Product product)
        {
            // demonstrate that the current ActionContext can be accessed by
            // controller actions
            string host = this.ActionContext.ControllerContext.Request.Headers.Host;
        }

        public void InsertOrderDetail(Order_Detail detail)
        {
        }
    }
}
