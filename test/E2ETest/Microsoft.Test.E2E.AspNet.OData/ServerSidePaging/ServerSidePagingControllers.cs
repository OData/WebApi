//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ServerSidePaging
{
    public class ServerSidePagingCustomersController : TestODataController
    {
        private readonly IList<ServerSidePagingCustomer> _serverSidePagingCustomers;

        public ServerSidePagingCustomersController()
        {
            _serverSidePagingCustomers = Enumerable.Range(1, 7)
                .Select(i => new ServerSidePagingCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i
                }).ToList();

            for (int i = 0; i < _serverSidePagingCustomers.Count; i++)
            {
                // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
                // NextPageLink will be expected on the Customers collection as well as
                // the Orders child collection on Customer 1
                _serverSidePagingCustomers[i].ServerSidePagingOrders = Enumerable.Range(1, 6 - i)
                    .Select(j => new ServerSidePagingOrder
                    {
                        Id = j,
                        Amount = (i + j) * 10,
                        ServerSidePagingCustomer = _serverSidePagingCustomers[i]
                    }).ToList();
            }
        }

        [EnableQuery(PageSize = 5)]
        public ITestActionResult Get()
        {
            return Ok(_serverSidePagingCustomers);
        }
    }

    public class ServerSidePagingEmployeesController : TestODataController
    {
        private static List<ServerSidePagingEmployee> employees = new List<ServerSidePagingEmployee>(
            Enumerable.Range(1, 13).Select(idx => new ServerSidePagingEmployee
            {
                Id = idx,
                HireDate = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2022, 11, 07).AddMonths(idx), TimeZoneInfo.Local)
            }));

        [EnableQuery(PageSize = 3)]
        public ITestActionResult GetEmployeesHiredInPeriod([FromODataUri] DateTime fromDate, [FromODataUri] DateTime toDate)
        {
            var hiredInPeriod = employees.Where(d => d.HireDate >= fromDate && d.HireDate <= toDate);

            return Ok(hiredInPeriod);
        }
    }
}
