// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.DollarQuery
{
    public class DollarQueryCustomersController : TestODataController
    {
        private IList<DollarQueryCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                new DollarQueryCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i,
                    Orders = Enumerable.Range(0, i).Select(j =>
                        new DollarQueryOrder
                        {
                            Id = i * 10 + j,
                            PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10 + j)),
                            Detail = "This is Order " + i * 10 + j
                        }).ToList(),
                    SpecialOrder = new DollarQueryOrder
                    {
                        Id = i * 10,
                        PurchaseDate = DateTime.Today.Subtract(TimeSpan.FromDays(i * 10)),
                        Detail = "This is Order " + i * 10
                    }
                }).ToList();

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get(int key)
        {
            var customer = customers.FirstOrDefault(c => c.Id == key);

            if (customer == null)
            {
                throw new ArgumentOutOfRangeException("key");
            }

            return Ok(customer);
        }
    }
}
