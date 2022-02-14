//-----------------------------------------------------------------------------
// <copyright file="OpenTypeControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.OpenType
{

    public class EmployeesController : TestODataController
    {
        public static IList<Employee> Employees = null;

        static EmployeesController()
        {
            Employees = InitEmployees();
        }

        public static IList<Employee> InitEmployees()
        {
            IList<Employee> employees = new List<Employee>();
            Employee employee = new Employee() { Id = 1, Name = "Name1" };
            employee.Account = AccountsController.Accounts.Single(a => a.Id == 1);
            employees.Add(employee);

            Manager mananger = new Manager { Id = 2, Name = "Name2", Heads = 1 };
            mananger.DynamicProperties.Add("Level", 1);
            mananger.DynamicProperties.Add("Gender", Gender.Male);
            mananger.DynamicProperties.Add("PhoneNumbers", new List<string>() { "8621-8888-8888", "8610-6666-6666" });
            employees.Add(mananger);

            return employees;
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(Employees);
        }

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            Employee employee = Employees.Single(e => e.Id == key);
            return Ok(employee);
        }

        [EnableQuery]
        public ITestActionResult Post([FromBody]Employee employee)
        {
            employee.Id = Employees.Count + 1;
            Employees.Add(employee);
            return Created(employee);
        }

        [EnableQuery]
        public ITestActionResult Put(int key, [FromBody]Employee employee)
        {
            Employee originalEmployee = Employees.Single(e => e.Id == key);
            employee.Id = key;
            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        [EnableQuery]
        public ITestActionResult Patch(int key, [FromBody]Delta<Employee> employee)
        {
            Employee originalEmployee = Employees.Single(e => e.Id == key);
            employee.Patch(originalEmployee);

            return Ok(employee);
        }

        [EnableQuery]
        public ITestActionResult GetEmployeesFromManager()
        {
            var managers = Employees.OfType<Manager>();
            return Ok(managers);
        }

        [EnableQuery]
        [ODataRoute("Employees/Microsoft.Test.E2E.AspNet.OData.OpenType.Manager({key})")]
        public ITestActionResult GetManager(int key)
        {
            Employee manager = Employees.OfType<Manager>().Single(e => e.Id == key);
            return Ok(manager);
        }

        [EnableQuery]
        public ITestActionResult PutManager(int key, [FromBody]Manager employee)
        {
            Manager originalEmployee = Employees.OfType<Manager>().Single(e => e.Id == key);
            employee.Id = key;

            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        [EnableQuery]
        public ITestActionResult PatchManager(int key, [FromBody]Delta<Manager> employee)
        {
            Manager originalEmployee = Employees.OfType<Manager>().Single(e => e.Id == key);
            employee.Patch(originalEmployee);

            return Ok(originalEmployee);
        }

        public ITestActionResult Delete(int key)
        {
            Employee employee = Employees.Single(e => e.Id == key);
            Employees.Remove(employee);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }

    #region  AccountsController

    public class AccountsController : TestODataController
    {
        static AccountsController()
        {
            InitAccounts();
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        public static IList<Account> Accounts = null;

        private static void InitAccounts()
        {
            Accounts = new List<Account>
            {
               
                new PremiumAccount()
                {
                    Id = 1,
                    Name = "Name1",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName1"
                    },
                    Address = new GlobalAddress()
                    {
                        City = "Redmond",
                        Street = "1 Microsoft Way",
                        CountryCode="US"
                    },
                    Tags = new Tags(),
                    Since=new DateTimeOffset(new DateTime(2014,5,22),TimeSpan.FromHours(8)),
                },
                new Account()
                {
                    Id = 2,
                    Name = "Name2",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName2"
                    },
                    Address =  new Address()
                    {
                        City = "Shanghai",
                        Street = "Zixing Road"
                    },
                    Tags = new Tags()
                },
                new Account()
                {
                    Id = 3,
                    Name = "Name3",
                    AccountInfo = new AccountInfo()
                    {
                        NickName = "NickName3"
                        
                    },
                    Address = new Address()
                    {
                        City = "Beijing",
                        Street = "Danling Street"
                    },
                    Tags = new Tags()
                }
            };

            Account account = Accounts.Single(a => a.Id == 1);
            account.DynamicProperties["OwnerAlias"] = "jinfutan";
            account.DynamicProperties["OwnerGender"] = Gender.Female;
            account.DynamicProperties["IsValid"] = true;
            account.DynamicProperties["ShipAddresses"] = new List<Address>(){
                new Address
                {
                    City = "Beijing",
                    Street = "Danling Street"
                },
                new Address
                {
                    City="Shanghai",
                    Street="Zixing",
                }
            };

            Accounts[0].AccountInfo.DynamicProperties["Age"] = 10;

            Accounts[0].AccountInfo.DynamicProperties["Gender"] = Gender.Male;

            Accounts[0].AccountInfo.DynamicProperties["Subs"] = new string[] { "Xbox", "Windows", "Office" };

            Accounts[0].Address.DynamicProperties["CountryOrRegion"] = "US";
            Accounts[0].Tags.DynamicProperties["Tag1"] = "Value 1";
            Accounts[0].Tags.DynamicProperties["Tag2"] = "Value 2";

            Accounts[1].AccountInfo.DynamicProperties["Age"] = 20;

            Accounts[1].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[1].AccountInfo.DynamicProperties["Subs"] = new string[] { "Xbox", "Windows" };

            Accounts[1].Address.DynamicProperties["CountryOrRegion"] = "China";
            Accounts[1].Tags.DynamicProperties["Tag1"] = "abc";

            Accounts[2].AccountInfo.DynamicProperties["Age"] = 30;

            Accounts[2].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[2].AccountInfo.DynamicProperties["Subs"] = new string[] { "Windows", "Office" };

            Accounts[2].Address.DynamicProperties["CountryOrRegion"] = "China";
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(Accounts.AsQueryable());
        }

        [EnableQuery]
        public ITestActionResult GetAccountsFromPremiumAccount()
        {
            return Ok(Accounts.OfType<PremiumAccount>().AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        [ODataRoute("Accounts")]
        public ITestActionResult GetAttributeRouting()
        {
            return Ok(Accounts.AsQueryable());
        }

        [HttpGet]
        public ITestActionResult Get(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }

        [HttpGet]
        [ODataRoute("Accounts({key})")]
        public ITestActionResult GetAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.PremiumAccount/Since")]
        public ITestActionResult GetSinceFromPremiumAccount(int key)
        {
            return Ok(Accounts.OfType<PremiumAccount>().SingleOrDefault(e => e.Id == key).Since);
        }

        public ITestActionResult GetAccountInfoFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).AccountInfo);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address")]
        public ITestActionResult GetAddressAttributeRouting(int key)
        {
            return GetAddress(key);
        }

        // convention routing
        public ITestActionResult GetAddress(int key)
        {
            Account account = Accounts.SingleOrDefault(e => e.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                return this.StatusCode(HttpStatusCode.NoContent);
            }

            return Ok(account.Address);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address/Microsoft.Test.E2E.AspNet.OData.OpenType.GlobalAddress")]
        public ITestActionResult GetGlobalAddress(int key)
        {
            Address address = Accounts.SingleOrDefault(e => e.Id == key).Address;
            return Ok(address as GlobalAddress);
        }

        [HttpGet]
        public ITestActionResult GetAddressOfGlobalAddressFromAccount(int key)
        {
            Address address = Accounts.SingleOrDefault(e => e.Id == key).Address;
            return Ok(address as GlobalAddress);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address/City")]
        public ITestActionResult GetCityAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Address.City);
        }

        public ITestActionResult GetTagsFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Tags")]
        public ITestActionResult GetTagsAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpPatch]
        public ITestActionResult Patch(int key, [FromBody]Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(a => a.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<Account> ifMatchAccounts = queryOptions.IfMatch.ApplyTo(appliedAccounts.AsQueryable()).Cast<Account>();

                if (ifMatchAccounts.Count() == 0)
                {
                    return BadRequest(string.Format("The entry with Id {0} has been updated", key));
                }
            }

            Account account = appliedAccounts.Single();
            patch.Patch(account);

            return Ok(account);
        }

        [HttpPatch]
        [ODataRoute("Accounts({key})")]
        public ITestActionResult PatchAttributeRouting(int key, [FromBody]Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(a => a.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<Account> ifMatchAccounts = queryOptions.IfMatch.ApplyTo(appliedAccounts.AsQueryable()).Cast<Account>();

                if (ifMatchAccounts.Count() == 0)
                {
                    return BadRequest(string.Format("The entry with Id {0} has been updated", key));
                }
            }

            Account account = appliedAccounts.Single();
            patch.Patch(account);

            return Ok(account);
        }

        [HttpPut]
        public ITestActionResult Put(int key, [FromBody]Account account)
        {
            if (key != account.Id)
            {
                return BadRequest("The ID of customer is not matched with the key");
            }

            Account originalAccount = Accounts.Where(a => a.Id == account.Id).Single();
            Accounts.Remove(originalAccount);
            Accounts.Add(account);
            return Ok(account);
        }

        [HttpPut]
        [ODataRoute("Accounts({key})")]
        public ITestActionResult PutAttributeRouting(int key, [FromBody]Account account)
        {
            if (key != account.Id)
            {
                return BadRequest("The ID of customer is not matched with the key");
            }

            Account originalAccount = Accounts.Where(a => a.Id == account.Id).Single();
            Accounts.Remove(originalAccount);
            Accounts.Add(account);
            return Ok(account);
        }

        [HttpPost]
        public ITestActionResult Post([FromBody]Account account)
        {
            account.Id = Accounts.Count + 1;
            account.DynamicProperties["OwnerGender"] = Gender.Male;// Defect 2371564 odata.type is missed in client payload for dynamic enum type
            Accounts.Add(account);

            return Created(account);
        }

        [HttpPost]
        [ODataRoute("Accounts")]
        public ITestActionResult PostAttributeRouting([FromBody]Account account)
        {
            account.Id = Accounts.Count + 1;
            Accounts.Add(account);

            return Created(account);
        }

        [HttpDelete]
        public ITestActionResult Delete(int key)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(c => c.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Account account = appliedAccounts.Single();
            Accounts.Remove(account);
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("Accounts({key})")]
        public ITestActionResult DeleteAttributeRouting(int key)
        {
            IEnumerable<Account> appliedAccounts = Accounts.Where(c => c.Id == key);

            if (appliedAccounts.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Account account = appliedAccounts.Single();
            Accounts.Remove(account);
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]
        public ITestActionResult PatchToAddress(int key, [FromBody]Delta<Address> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new Address();
            }

            account.Address = address.Patch(account.Address);

            return Updated(account);
        }

        [HttpPatch]
        public ITestActionResult PatchToAddressOfGlobalAddress(int key, [FromBody]Delta<GlobalAddress> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new GlobalAddress();
            }

            GlobalAddress globalAddress = account.Address as GlobalAddress;
            address.Patch(globalAddress);
            return Updated(account);
        }

        [HttpPut]
        public ITestActionResult PutToAddress(int key, [FromBody]Delta<Address> address)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            if (account.Address == null)
            {
                account.Address = new Address();
            }

            address.Put(account.Address);

            return Updated(account);
        }

        public ITestActionResult DeleteToAddress(int key)
        {
            Account account = Accounts.FirstOrDefault(a => a.Id == key);
            if (account == null)
            {
                return NotFound();
            }

            account.Address = null;
            return Updated(account);
        }

        #region Function & Action

        [HttpGet]
        public Address GetAddressFunctionOnAccount(int key)
        {
            return Accounts.SingleOrDefault(e => e.Id == key).Address;
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.GetAddressFunction()")]
        public Address GetAddressFunctionAttributeRouting(int key)
        {
            return Accounts.SingleOrDefault(e => e.Id == key).Address;
        }

        [HttpPost]
        public AccountInfo IncreaseAgeActionOnAccount(int key)
        {
            AccountInfo accountInfo = Accounts.SingleOrDefault(e => e.Id == key).AccountInfo;
            accountInfo.DynamicProperties["Age"] = (int)accountInfo.DynamicProperties["Age"] + 1;
            return accountInfo;
        }

        [HttpPost]
        [ODataRoute("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.IncreaseAgeAction()")]
        public AccountInfo IncreaseAgeActionAttributeRouting(int key)
        {
            AccountInfo accountInfo = Accounts.SingleOrDefault(e => e.Id == key).AccountInfo;
            accountInfo.DynamicProperties["Age"] = (int)accountInfo.DynamicProperties["Age"] + 1;
            return accountInfo;
        }

        [HttpPost]
        [ODataRoute("UpdateAddressAction")]
        public Address UpdateAddressActionAttributeRouting([FromBody]ODataActionParameters parameters)
        {
            var id = (int)parameters["ID"];
            var address = parameters["Address"] as Address;

            Account account = Accounts.Single(a => a.Id == id);
            account.Address = address;
            return address;
        }

        [HttpPost]
        [ODataRoute("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.AddShipAddress")]
        public ITestActionResult AddShipAddress(int key, [FromBody]ODataActionParameters parameters)
        {
            Account account = Accounts.Single(c => c.Id == key);
            if (account.DynamicProperties["ShipAddresses"] == null)
            {
                account.DynamicProperties["ShipAddresses"] = new List<Address>();
            }

            IList<Address> addresses = (IList<Address>)account.DynamicProperties["ShipAddresses"];
            addresses.Add(parameters["address"] as Address);
            return Ok(addresses.Count);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Microsoft.Test.E2E.AspNet.OData.OpenType.GetShipAddresses")]
        public ITestActionResult GetShipAddresses(int key)
        {
            Account account = Accounts.Single(c => c.Id == key);
            if (account.DynamicProperties["ShipAddresses"] == null)
            {
                return Ok(new List<Address>());
            }
            else
            {
                IList<Address> addresses = (IList<Address>)account.DynamicProperties["ShipAddresses"];
                return Ok(addresses);
            }
        }
        #endregion

        [HttpPost]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            InitAccounts();
            EmployeesController.Employees = EmployeesController.InitEmployees();
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }

    #endregion
}
