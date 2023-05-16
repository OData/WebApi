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
using Microsoft.AspNet.OData.Routing;
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

    public class SkipTokenPagingS1CustomersController : TestODataController
    {
        private static readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
        {
            new SkipTokenPagingCustomer { Id = 1, CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 2, CreditLimit = 2 },
            new SkipTokenPagingCustomer { Id = 3, CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 4, CreditLimit = 30 },
            new SkipTokenPagingCustomer { Id = 5, CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 6, CreditLimit = 35 },
            new SkipTokenPagingCustomer { Id = 7, CreditLimit = 5 },
            new SkipTokenPagingCustomer { Id = 8, CreditLimit = 50 },
            new SkipTokenPagingCustomer { Id = 9, CreditLimit = 25 },
        };

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }
    }

    public class SkipTokenPagingS2CustomersController : TestODataController
    {
        private readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
        {
            new SkipTokenPagingCustomer { Id = 1, Grade = "A", CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 2, Grade = "B", CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 3, Grade = "A", CreditLimit = 10 },
            new SkipTokenPagingCustomer { Id = 4, Grade = "C", CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 5, Grade = "A", CreditLimit = 30 },
            new SkipTokenPagingCustomer { Id = 6, Grade = "C", CreditLimit = null },
            new SkipTokenPagingCustomer { Id = 7, Grade = "B", CreditLimit = 5 },
            new SkipTokenPagingCustomer { Id = 8, Grade = "C", CreditLimit = 25 },
            new SkipTokenPagingCustomer { Id = 9, Grade = "B", CreditLimit = 50 },
            new SkipTokenPagingCustomer { Id = 10, Grade = "D", CreditLimit = 50 },
            new SkipTokenPagingCustomer { Id = 11, Grade = "F", CreditLimit = 35 },
            new SkipTokenPagingCustomer { Id = 12, Grade = "F", CreditLimit = 30 },
            new SkipTokenPagingCustomer { Id = 13, Grade = "F", CreditLimit = 55 }
        };

        [EnableQuery(PageSize = 4)]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }
    }

    public class SkipTokenPagingS3CustomersController : TestODataController
    {
        private static readonly List<SkipTokenPagingCustomer> customers = new List<SkipTokenPagingCustomer>
        {
            new SkipTokenPagingCustomer { Id = 1, CustomerSince = null },
            new SkipTokenPagingCustomer { Id = 2, CustomerSince = new DateTime(2023, 1, 2) },
            new SkipTokenPagingCustomer { Id = 3, CustomerSince = null },
            new SkipTokenPagingCustomer { Id = 4, CustomerSince = new DateTime(2023, 1, 30) },
            new SkipTokenPagingCustomer { Id = 5, CustomerSince = null },
            new SkipTokenPagingCustomer { Id = 6, CustomerSince = new DateTime(2023, 2, 4) },
            new SkipTokenPagingCustomer { Id = 7, CustomerSince = new DateTime(2023, 1, 5) },
            new SkipTokenPagingCustomer { Id = 8, CustomerSince = new DateTime(2023, 2, 19) },
            new SkipTokenPagingCustomer { Id = 9, CustomerSince = new DateTime(2023, 1, 25) },
        };

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }
    }

    public class SkipTokenPagingEdgeCase1CustomersController : TestODataController
    {
        private static readonly List<SkipTokenPagingEdgeCase1Customer> customers = new List<SkipTokenPagingEdgeCase1Customer>
        {
            new SkipTokenPagingEdgeCase1Customer { Id = 2, CreditLimit = 2 },
            new SkipTokenPagingEdgeCase1Customer { Id = 4, CreditLimit = 30 },
            new SkipTokenPagingEdgeCase1Customer { Id = 6, CreditLimit = 35 },
            new SkipTokenPagingEdgeCase1Customer { Id = 7, CreditLimit = 5 },
            new SkipTokenPagingEdgeCase1Customer { Id = 9, CreditLimit = 25 },
        };

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }
    }

    public class ContainmentPagingCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(ContainmentPagingDataSource.Customers);
        }

        [EnableQuery(PageSize = 2)]
        public ITestActionResult GetOrders(int key)
        {
            var customer = ContainmentPagingDataSource.Customers.SingleOrDefault(d => d.Id == key);

            if (customer == null)
            {
                return BadRequest();
            }

            return Ok(customer.Orders);
        }
    }

    public class ContainmentPagingCompanyController : TestODataController
    {
        private static readonly ContainmentPagingCustomer company = new ContainmentPagingCustomer
        {
            Id = 1,
            Orders = ContainmentPagingDataSource.Orders.Take(ContainmentPagingDataSource.TargetSize).ToList()
        };

        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(company);
        }

        [EnableQuery(PageSize = 2)]
        public ITestActionResult GetOrders()
        {
            return Ok(company.Orders);
        }
    }

    public class NoContainmentPagingCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 2)]
        public ITestActionResult Get()
        {
            return Ok(NoContainmentPagingDataSource.Customers);
        }

        [EnableQuery(PageSize = 2)]
        public ITestActionResult GetOrders(int key)
        {
            var customer = NoContainmentPagingDataSource.Customers.SingleOrDefault(d => d.Id == key);

            if (customer == null)
            {
                return BadRequest();
            }

            return Ok(customer.Orders);
        }
    }

    public class ContainmentPagingMenusController : TestODataController
    {
        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult Get()
        {
            return Ok(ContainmentPagingDataSource.Menus);
        }

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult GetFromContainmentPagingExtendedMenu()
        {
            return Ok(ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>());
        }

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult GetTabsFromContainmentPagingExtendedMenu(int key)
        {
            var menu = ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>().SingleOrDefault(d => d.Id == key);

            if (menu == null)
            {
                return BadRequest();
            }

            return Ok(menu.Tabs);
        }

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult GetPanelsFromContainmentPagingExtendedMenu(int key)
        {
            var menu = ContainmentPagingDataSource.Menus.OfType<ContainmentPagingExtendedMenu>().SingleOrDefault(d => d.Id == key);

            if (menu == null)
            {
                return BadRequest();
            }

            return Ok(menu.Panels);
        }
    }

    public class ContainmentPagingRibbonController : TestODataController
    {
        private static readonly ContainmentPagingMenu ribbon = new ContainmentPagingExtendedMenu
        {
            Id = 1,
            Tabs = ContainmentPagingDataSource.Tabs.Take(ContainmentPagingDataSource.TargetSize).ToList()
        };

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult Get()
        {
            return Ok(ribbon);
        }

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        public ITestActionResult GetFromContainmentPagingExtendedMenu()
        {
            return Ok(ribbon as ContainmentPagingExtendedMenu);
        }

        [EnableQuery(PageSize = 2, MaxExpansionDepth = 4)]
        [HttpGet]
        [ODataRoute("ContainmentPagingRibbon/Microsoft.Test.E2E.AspNet.OData.ServerSidePaging.ContainmentPagingExtendedMenu/Tabs")]
        public ITestActionResult GetTabsFromContainmentPagingExtendedMenu()
        {
            return Ok((ribbon as ContainmentPagingExtendedMenu).Tabs);
        }
    }
}
