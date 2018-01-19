// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    public class ItemsController : ODataController
    {
        private static readonly OrderByEdmModel.OrderByContext Db = new OrderByEdmModel.OrderByContext();

        static ItemsController()
        {
            if (Db.Items.Any() && Db.Items2.Any())
            {
                return;
            }

            Db.Items.Add(new Item() { A = 1, C = 1, B = 99, Name = "#1 - A1 C1 B99" });
            Db.Items.Add(new Item() { A = 1, C = 2, B = 98, Name = "#2 - A1 C2 B98" });
            Db.Items.Add(new Item() { A = 1, C = 3, B = 97, Name = "#3 - A1 C3 B97" });
            Db.Items.Add(new Item() { A = 1, C = 4, B = 96, Name = "#4 - A1 C4 B96" });


            Db.Items2.Add(new Item2() { A = "AA", C = "BB", B = 99, Name = "#2" });
            Db.Items2.Add(new Item2() { A = "BB", C = "AA", B = 98, Name = "#1" });
            Db.Items2.Add(new Item2() { A = "01", C = "XX", B = 1, Name = "#3" });
            Db.Items2.Add(new Item2() { A = "00", C = "ZZ", B = 96, Name = "#4" });
            Db.SaveChanges();
        }

        [EnableQuery]
        public IHttpActionResult GetItems()
        {
            return Ok(Db.Items);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("Items2")]
        public IHttpActionResult GetItems2()
        {
            return Ok(Db.Items2);
        }

    }
}