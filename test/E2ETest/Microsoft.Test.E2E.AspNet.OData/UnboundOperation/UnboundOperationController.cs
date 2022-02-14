//-----------------------------------------------------------------------------
// <copyright file="UnboundOperationController.cs" company=".NET Foundation">
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
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.UnboundOperation
{
    public class ConventionCustomersController : TestODataController
    {
        private static IList<ConventionCustomer> _customers = null;

        private void InitCustomers()
        {
            _customers = Enumerable.Range(1, 10).Select(i =>
            new ConventionCustomer
            {
                ID = 400 + i,
                Name = "Name " + i,
                Address = new ConventionAddress()
                {
                    Street = "Street " + i,
                    City = "City " + i,
                    ZipCode = (201100 + i).ToString()
                },
                Orders = Enumerable.Range(1, i).Select(j =>
                new ConventionOrder
                {
                    ID = j,
                    OrderName = "OrderName " + j,
                    Price = j,
                    OrderGuid = Guid.Empty
                }).ToList()
            }).ToList();
        }

        public ConventionCustomersController()
        {
            if (_customers == null)
                InitCustomers();
        }

        public IList<ConventionCustomer> Customers { get { return _customers; } }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(_customers.AsQueryable());
        }

        // It's a top level function without parameters
        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetAllConventionCustomers()")]
        [ODataRoute("GetAllConventionCustomersImport()")]
        public IEnumerable<ConventionCustomer> GetAllConventionCustomers()
        {
            return _customers;
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetAllConventionCustomersImport(CustomerName={customerName})")]
        [ODataRoute("GetAllConventionCustomersImport(CustomerName={customerName})/$count")]
        // [FromODataUri] can not be deleted within below line, or the value of OrderName will be enclosed by single quote mark('). 
        public IEnumerable<ConventionCustomer> GetAllConventionCustomers([FromODataUri]String CustomerName)
        {
            IEnumerable<ConventionCustomer> customers = _customers.Where(c => c.Name.Contains(CustomerName));
            return customers;
        }

        // It's a top level function with one parameter
        [ODataRoute("GetConventionCustomerById(CustomerId={CustomerId})")]
        [ODataRoute("GetConventionCustomerByIdImport(CustomerId={CustomerId})")]
        public ConventionCustomer GetConventionCustomerById(int CustomerId)
        {
            return _customers.Where(c => c.ID == CustomerId).FirstOrDefault();
        }

        [ODataRoute("GetConventionCustomerNameByIdImport(CustomerId={CustomerId})")]
        [ODataRoute("GetConventionCustomerByIdImport(CustomerId={CustomerId})/Name")]
        public String GetConventionCustomerNameById([FromODataUri]int CustomerId)
        {
            return _customers.Where(c => c.ID == CustomerId).FirstOrDefault().Name;
        }

        [ODataRoute("GetConventionOrderByCustomerIdAndOrderName(CustomerId={CustomerId},OrderName={OrderName})")]
        [ODataRoute("GetConventionOrderByCustomerIdAndOrderNameImport(CustomerId={CustomerId},OrderName={OrderName})")]
        public ConventionOrder GetConventionOrderByCustomerIdAndOrderName(int CustomerId, [FromODataUri]string OrderName)
        {
            ConventionCustomer customer = _customers.Where(c => c.ID == CustomerId).FirstOrDefault();
            return customer.Orders.Where(o => o.OrderName == OrderName).FirstOrDefault();
        }

        [HttpGet]
        [ODataRoute("AdvancedFunction(nums={numbers},genders={genders},location={address},addresses={addresses},customer={customer},customers={customers})")]
        public bool AdvancedFunction([FromODataUri]IEnumerable<int> numbers,
            [FromODataUri]IEnumerable<ConventionGender> genders,
            [FromODataUri]ConventionAddress address, [FromODataUri]IEnumerable<ConventionAddress> addresses,
            [FromODataUri]ConventionCustomer customer, [FromODataUri]IEnumerable<ConventionCustomer> customers)
        {
            Assert.Equal(new[] {1, 2, 3}, numbers);
            Assert.Equal(new[] {ConventionGender.Male, ConventionGender.Female}, genders);

            IEnumerable<ConventionAddress> newAddress = addresses.Concat(new[] {address});
            Assert.Equal(2, newAddress.Count());
            foreach (ConventionAddress addr in newAddress)
            {
                Assert.Equal("Zi Xin Rd.", addr.Street);
                Assert.Equal("Shanghai", addr.City);
                Assert.Equal("2001100", addr.ZipCode);
            }

            IEnumerable<ConventionCustomer> newCustomers = customers.Concat(new[] { customer});
            Assert.Equal(2, newCustomers.Count());
            foreach (ConventionCustomer cust in newCustomers)
            {
                Assert.Equal(7, cust.ID);
                Assert.Equal("Tony", cust.Name);
                Assert.Null(cust.Address);
            }

            return true;
        }

        [EnableQuery]
        [ODataRoute("GetDefinedGenders()")]
        [ODataRoute("GetDefinedGenders()/$count")]
        public ITestActionResult GetDefinedGenders()
        {
            IList<ConventionGender> genders = new List<ConventionGender>();
            genders.Add(ConventionGender.Male);
            genders.Add(ConventionGender.Female);
            return Ok(genders);
        }

        [HttpPost]
        [ODataRoute("ResetDataSource")]
        [ODataRoute("ResetDataSourceImport")]
        public ITestActionResult ResetDataSource()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            this.InitCustomers();

            return Ok();
        }

        [HttpPost]
        [EnableQuery]
        [ODataRoute("UpdateAddress")]
        [ODataRoute("UpdateAddressImport")]
        public ITestActionResult UpdateAddress([FromBody]ODataUntypedActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = (int)parameters["ID"];
            var address = parameters["Address"] as EdmComplexObject;
            var conventionAddress = new ConventionAddress();
            object temp = null;
            if (address.TryGetPropertyValue("Street", out temp))
            {
                conventionAddress.Street = temp.ToString();
            }
            if (address.TryGetPropertyValue("City", out temp))
            {
                conventionAddress.City = temp.ToString();
            }
            if (address.TryGetPropertyValue("ZipCode", out temp))
            {
                conventionAddress.ZipCode = temp.ToString();
            }
            ConventionCustomer customer = _customers.Where(c => c.ID == id).FirstOrDefault();
            customer.Address = conventionAddress;
            return Ok(_customers);
        }

        /*
        [HttpPost]
        [ODataRoute("CreateCustomer")]
        [ODataRoute("CreateCustomerImport")]
        public ITestActionResult CreateCustomer(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var conventionCustomer =(ConventionCustomer) parameters["value"];
            conventionCustomer.ID = _customers.Count() + 1;
            _customers.Add(conventionCustomer);
            return Ok(_customers);
        }
         * */

        [HttpPost]
        [ODataRoute("AdvancedAction")]
        public ITestActionResult AdvancedAction([FromBody]ODataActionParameters parameters)
        {
            Assert.NotNull(parameters);
            Assert.Equal(new[] { 4, 5, 6 }, parameters["nums"] as IEnumerable<int>);
            Assert.Equal(new[] { ConventionGender.Male, ConventionGender.Female }, parameters["genders"] as IEnumerable<ConventionGender>);

            IList<ConventionAddress> newAddress = (parameters["addresses"] as IEnumerable<ConventionAddress>).ToList();
            Assert.Single(newAddress);
            foreach (ConventionAddress addr in newAddress.Concat(new[] {parameters["location"]}))
            {
                Assert.Equal("NY Rd.", addr.Street);
                Assert.Equal("Redmond", addr.City);
                Assert.Equal("9011", addr.ZipCode);
            }

            IList<ConventionCustomer> newCustomers = (parameters["customers"] as IEnumerable<ConventionCustomer>).ToList();
            Assert.Single(newAddress);
            foreach (ConventionCustomer cust in newCustomers.Concat(new[] { parameters["customer"] }))
            {
                Assert.Equal(8, cust.ID);
                Assert.Equal("Mike", cust.Name);
                Assert.Null(cust.Address);
            }

            return Ok();
        }
    }
}
