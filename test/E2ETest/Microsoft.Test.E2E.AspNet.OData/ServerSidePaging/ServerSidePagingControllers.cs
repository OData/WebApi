// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            _serverSidePagingCustomers = new List<ServerSidePagingCustomer>(
                Enumerable.Range(1, 7).Select(i => new ServerSidePagingCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i
                }));

            for (int i = 0; i < _serverSidePagingCustomers.Count; i++)
            {
                // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
                // NextPageLink will be expected on the Customers collection as well as
                // the Orders child collection on Customer 1
                _serverSidePagingCustomers[i].ServerSidePagingOrders = new List<ServerSidePagingOrder>(
                    Enumerable.Range(1, 6 - i).Select(j => new ServerSidePagingOrder
                    {
                        Id = j,
                        Amount = (i + j) * 10,
                        ServerSidePagingCustomer = _serverSidePagingCustomers[i]
                    }));
            }
        }

        [EnableQuery(PageSize = 5)]
        public ITestActionResult Get()
        {
            return Ok(_serverSidePagingCustomers);
        }
    }
}
