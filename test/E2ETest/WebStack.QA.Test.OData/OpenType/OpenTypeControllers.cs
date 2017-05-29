namespace WebStack.QA.Test.OData.OpenType
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    public class EmployeesController : ODataController
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
        public IHttpActionResult Get()
        {
            return Ok(Employees);
        }

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            Employee employee = Employees.Single(e => e.Id == key);
            return Ok(employee);
        }

        [EnableQuery]
        public IHttpActionResult Post(Employee employee)
        {
            employee.Id = Employees.Count + 1;
            Employees.Add(employee);
            return Created(employee);
        }

        [EnableQuery]
        public IHttpActionResult Put(int key, Employee employee)
        {
            Employee originalEmployee = Employees.Single(e => e.Id == key);
            employee.Id = key;
            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        [EnableQuery]
        public IHttpActionResult Patch(int key, Delta<Employee> employee)
        {
            Employee originalEmployee = Employees.Single(e => e.Id == key);
            employee.Patch(originalEmployee);

            return Ok(employee);
        }

        [EnableQuery]
        public IHttpActionResult GetEmployeesFromManager()
        {
            var managers = Employees.OfType<Manager>();
            return Ok(managers);
        }

        [EnableQuery]
        [ODataRoute("Employees/WebStack.QA.Test.OData.OpenType.Manager({key})")]
        public IHttpActionResult GetManager(int key)
        {
            Employee manager = Employees.OfType<Manager>().Single(e => e.Id == key);
            return Ok(manager);
        }

        [EnableQuery]
        public IHttpActionResult PutManager(int key, Manager employee)
        {
            Manager originalEmployee = Employees.OfType<Manager>().Single(e => e.Id == key);
            employee.Id = key;

            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        [EnableQuery]
        public IHttpActionResult PatchManager(int key, Delta<Manager> employee)
        {
            Manager originalEmployee = Employees.OfType<Manager>().Single(e => e.Id == key);
            employee.Patch(originalEmployee);

            return Ok(originalEmployee);
        }

        public IHttpActionResult Delete(int key)
        {
            Employee employee = Employees.Single(e => e.Id == key);
            Employees.Remove(employee);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }

    #region  AccountsController

    public class AccountsController : ODataController
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

            Accounts[0].Address.DynamicProperties["Country"] = "US";
            Accounts[0].Tags.DynamicProperties["Tag1"] = "Value 1";
            Accounts[0].Tags.DynamicProperties["Tag2"] = "Value 2";

            Accounts[1].AccountInfo.DynamicProperties["Age"] = 20;

            Accounts[1].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[1].AccountInfo.DynamicProperties["Subs"] = new string[] { "Xbox", "Windows" };

            Accounts[1].Address.DynamicProperties["Country"] = "China";
            Accounts[1].Tags.DynamicProperties["Tag1"] = "abc";

            Accounts[2].AccountInfo.DynamicProperties["Age"] = 30;

            Accounts[2].AccountInfo.DynamicProperties["Gender"] = Gender.Female;

            Accounts[2].AccountInfo.DynamicProperties["Subs"] = new string[] { "Windows", "Office" };

            Accounts[2].Address.DynamicProperties["Country"] = "China";
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(Accounts.AsQueryable());
        }

        [EnableQuery]
        public IHttpActionResult GetAccountsFromPremiumAccount()
        {
            return Ok(Accounts.OfType<PremiumAccount>().AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        [ODataRoute("Accounts")]
        public IHttpActionResult GetAttributeRouting()
        {
            return Ok(Accounts.AsQueryable());
        }

        [HttpGet]
        public IHttpActionResult Get(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }

        [HttpGet]
        [ODataRoute("Accounts({key})")]
        public IHttpActionResult GetAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key));
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.OpenType.PremiumAccount/Since")]
        public IHttpActionResult GetSinceFromPremiumAccount(int key)
        {
            return Ok(Accounts.OfType<PremiumAccount>().SingleOrDefault(e => e.Id == key).Since);
        }

        public IHttpActionResult GetAccountInfoFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).AccountInfo);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address")]
        public IHttpActionResult GetAddressAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Address);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address/WebStack.QA.Test.OData.OpenType.GlobalAddress")]
        public IHttpActionResult GetGlobalAddress(int key)
        {
            Address address = Accounts.SingleOrDefault(e => e.Id == key).Address;
            return Ok(address as GlobalAddress);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Address/City")]
        public IHttpActionResult GetCityAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Address.City);
        }

        public IHttpActionResult GetTagsFromAccount(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/Tags")]
        public IHttpActionResult GetTagsAttributeRouting(int key)
        {
            return Ok(Accounts.SingleOrDefault(e => e.Id == key).Tags);
        }

        [HttpPatch]
        public IHttpActionResult Patch(int key, Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
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
        public IHttpActionResult PatchAttributeRouting(int key, Delta<Account> patch, ODataQueryOptions<Account> queryOptions)
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
        public IHttpActionResult Put(int key, Account account)
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
        public IHttpActionResult PutAttributeRouting(int key, Account account)
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
        public IHttpActionResult Post(Account account)
        {
            account.Id = Accounts.Count + 1;
            account.DynamicProperties["OwnerGender"] = Gender.Male;// Defect 2371564 odata.type is missed in client payload for dynamic enum type
            Accounts.Add(account);

            return Created(account);
        }

        [HttpPost]
        [ODataRoute("Accounts")]
        public IHttpActionResult PostAttributeRouting(Account account)
        {
            account.Id = Accounts.Count + 1;
            Accounts.Add(account);

            return Created(account);
        }

        [HttpDelete]
        public IHttpActionResult Delete(int key)
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
        public IHttpActionResult DeleteAttributeRouting(int key)
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

        #region Function & Action

        [HttpGet]
        public Address GetAddressFunctionOnAccount(int key)
        {
            return Accounts.SingleOrDefault(e => e.Id == key).Address;
        }

        [HttpGet]
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.OpenType.GetAddressFunction()")]
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
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.OpenType.IncreaseAgeAction()")]
        public AccountInfo IncreaseAgeActionAttributeRouting(int key)
        {
            AccountInfo accountInfo = Accounts.SingleOrDefault(e => e.Id == key).AccountInfo;
            accountInfo.DynamicProperties["Age"] = (int)accountInfo.DynamicProperties["Age"] + 1;
            return accountInfo;
        }

        [HttpPost]
        [ODataRoute("UpdateAddressAction")]
        public Address UpdateAddressActionAttributeRouting(ODataActionParameters parameters)
        {
            var id = (int)parameters["ID"];
            var address = parameters["Address"] as Address;

            Account account = Accounts.Single(a => a.Id == id);
            account.Address = address;
            return address;
        }

        [HttpPost]
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.OpenType.AddShipAddress")]
        public IHttpActionResult AddShipAddress(int key, ODataActionParameters parameters)
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
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.OpenType.GetShipAddresses")]
        public IHttpActionResult GetShipAddresses(int key)
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
        public IHttpActionResult ResetDataSource()
        {
            InitAccounts();
            EmployeesController.Employees = EmployeesController.InitEmployees();
            return this.StatusCode(HttpStatusCode.NoContent);
        }
    }

    #endregion
}

