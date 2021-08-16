//-----------------------------------------------------------------------------
// <copyright file="OrderByController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class ItemsController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private static readonly OrderByEdmModel.OrderByContext Db = new OrderByEdmModel.OrderByContext();
        private static readonly IQueryable<ItemWithoutColumn> _itemWithoutColumns;
        static ItemsController()
        {
            _itemWithoutColumns = new List<ItemWithoutColumn>()
            {
                // The key is A, B, C
                new ItemWithoutColumn() { A = 2, B = 1, C = 1, ExpectedOrder = 4 },
                new ItemWithoutColumn() { A = 1, B = 2, C = 1, ExpectedOrder = 3 },
                new ItemWithoutColumn() { A = 1, B = 1, C = 2, ExpectedOrder = 2 },
                new ItemWithoutColumn() { A = 1, B = 1, C = 1, ExpectedOrder = 1 }
            }.AsQueryable();

            if (Db.Items.Any() && Db.Items2.Any() && Db.ItemsWithEnum.Any())
            {
                return;
            }

            AddInSet(
                Db.Items,
                // The key is C, A, B
                new Item() { A = 1, B = 99, C = 2, ExpectedOrder = 4 },
                new Item() { A = 2, B = 2, C = 1, ExpectedOrder = 3 },
                new Item() { A = 2, B = 1, C = 1, ExpectedOrder = 2 },
                new Item() { A = 1, B = 96, C = 1, ExpectedOrder = 1 }
                );
            AddInSet(
                Db.Items2,
                // The key is C, B, A
                new Item2() { A = "AA", C = "BB", B = 99, ExpectedOrder = 2 },
                new Item2() { A = "BB", C = "AA", B = 98, ExpectedOrder = 1 },
                new Item2() { A = "01", C = "XX", B = 1, ExpectedOrder = 3 },
                new Item2() { A = "00", C = "ZZ", B = 96, ExpectedOrder = 4 }
                );

            AddInSet(
                Db.ItemsWithEnum,
                // The key is C, B, A
                new ItemWithEnum() { A = SmallNumber.One, B = "A", C = SmallNumber.One, ExpectedOrder = 1 },
                new ItemWithEnum() { A = SmallNumber.One, B = "B", C = SmallNumber.One, ExpectedOrder = 3 },
                new ItemWithEnum() { A = SmallNumber.One, B = "B", C = SmallNumber.Two, ExpectedOrder = 4 },
                new ItemWithEnum() { A = SmallNumber.Two, B = "A", C = SmallNumber.One, ExpectedOrder = 2 }
            );
            Db.SaveChanges();
        }

        private static void AddInSet<T>(IDbSet<T> set, params T[] items) where T : class
        {            
            foreach (var item in items)
            {
                set.Add(item);
            }
        }

        [EnableQuery]
        public ITestActionResult GetItems()
        {
            return Ok(Db.Items);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("Items2")]
        public ITestActionResult GetItems2()
        {
            return Ok(Db.Items2);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("ItemsWithEnum")]
        public ITestActionResult GetItemsWithEnum()
        {
            return Ok(Db.ItemsWithEnum);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("ItemsWithoutColumn")]
        public ITestActionResult GetItemsWithoutColumn()
        {
            return Ok(_itemWithoutColumns);
        }

#if NETCORE
        public void Dispose()
        {
            // Db.Dispose();
        }
#endif
    }
}
