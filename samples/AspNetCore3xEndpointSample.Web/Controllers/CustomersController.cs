// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using AspNetCore3xEndpointSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore3xEndpointSample.Web.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly CustomerOrderContext _context;

        public CustomersController(CustomerOrderContext context)
        {
            _context = context;

            if (_context.Customers.Count() == 0)
            {
                IList<Customer> customers = new List<Customer>
                {
                    new Customer
                    {
                        Name = "Jonier",
                        HomeAddress = new Address { City = "Redmond", Street = "156 AVE NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "Redmond", Street = "256 AVE NE"},
                            new Address { City = "Redd", Street = "56 AVE NE"},
                        },
                        Order = new Order { Title = "104m" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "abc" + e }).ToList()
                    },
                    new Customer
                    {
                        Name = "Sam",
                        HomeAddress = new Address { City = "Bellevue", Street = "Main St NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "Red4ond", Street = "456 AVE NE"},
                            new Address { City = "Re4d", Street = "51 NE"},
                        },
                        Order = new Order { Title = "Zhang" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "xyz" + e }).ToList()
                    },
                    new Customer
                    {
                        Name = "Peter",
                        HomeAddress = new Address {  City = "Hollewye", Street = "Main St NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "R4mond", Street = "546 NE"},
                            new Address { City = "R4d", Street = "546 AVE"},
                        },
                        Order = new Order { Title = "Jichan" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "ijk" + e }).ToList()
                    },
                };

                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                    _context.Orders.Add(customer.Order);
                    _context.Orders.AddRange(customer.Orders);
                }

                _context.SaveChanges();
            }
        }

        [EnableQuery]
        public IActionResult Get()
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Ok(_context.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_context.Customers.FirstOrDefault(c => c.Id == key));
        }
    }

    public class ImageAdsController : ODataController
    {
        private IList<ImageAd> imageAds = new List<ImageAd>
        {
            new ImageAd
            {
                Id = 1,
                Images = new List<Image>
                {
                    new Image { Id = 11, Name = "abc" },
                    new Image { Id = 12, Name = "xyz" }
                }
            },
            new SubImageAd
            {
                Id = 2,
                Images = new List<Image>
                {
                    new Image { Id = 21, Name = "ijk" },
                    new Image { Id = 22, Name = "efg" }
                },
                SubImages = new List<Image>
                {
                    new Image { Id = 31, Name = "bcd" },
                    new Image { Id = 42, Name = "opq" }
                },
            }
        };

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(imageAds);
        }
    }
}
