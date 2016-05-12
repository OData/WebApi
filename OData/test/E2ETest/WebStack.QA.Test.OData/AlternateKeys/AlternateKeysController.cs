using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Xunit;

namespace WebStack.QA.Test.OData.AlternateKeys
{
    public class CustomersController : ODataController
    {
        public IHttpActionResult Get(int key)
        {
            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(customer);
                    }
                }
            }

            return NotFound();
        }

        public IHttpActionResult Get(string keySSN)
        {
            return GetCustomerBySSN(keySSN);
        }

        // alternate key: SSN
        [HttpGet]
        [ODataRoute("Customers(SSN={ssn})")]
        public IHttpActionResult GetCustomerBySSN([FromODataUri]string ssn)
        {
            // for special test
            if (ssn == "special-SSN")
            {
                return Ok(ssn);
            }

            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("SSN", out value))
                {
                    string stringKey = (string)value;
                    if (ssn == stringKey)
                    {
                        return Ok(customer);
                    }
                }
            }

            return NotFound();
        }

        [HttpPatch]
        [ODataRoute("Customers(SSN={ssnKey})")]
        public IHttpActionResult PatchCustomerBySSN([FromODataUri]string ssnKey, EdmEntityObject delta)
        {
            Assert.Equal("SSN-6-T-006", ssnKey);

            IList<string> changedPropertyNames = delta.GetChangedPropertyNames().ToList();
            Assert.Equal(1, changedPropertyNames.Count);
            Assert.Equal("Name", String.Join(",", changedPropertyNames));

            IEdmEntityObject originalCustomer = null;
            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("SSN", out value))
                {
                    string stringKey = (string)value;
                    if (ssnKey == stringKey)
                    {
                        originalCustomer = customer;
                    }
                }
            }

            if (originalCustomer == null)
            {
                return NotFound();
            }

            object nameValue;
            delta.TryGetPropertyValue("Name", out nameValue);
            Assert.NotNull(nameValue);
            string strName = Assert.IsType<string>(nameValue);
            dynamic original = originalCustomer;
            original.Name = strName;

            return Ok(originalCustomer);
        }
    }

    public class OrdersController : ODataController
    {
        [HttpGet]
        [ODataRoute("Orders({orderKey})")]
        public IHttpActionResult GetOrderByPrimitiveKey(int orderKey)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("OrderId", out value))
                {
                    int intKey = (int)value;
                    if (orderKey == intKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet]
        [ODataRoute("Orders(Name={orderName})")]
        public IHttpActionResult GetOrderByName([FromODataUri]string orderName)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("Name", out value))
                {
                    string stringKey = (string)value;
                    if (orderName == stringKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet]
        [ODataRoute("Orders(Token={token})")]
        public IHttpActionResult GetOrderByToken([FromODataUri]Guid token)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("Token", out value))
                {
                    Guid guidKey = (Guid)value;
                    if (token == guidKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }
    }

    public class PeopleController : ODataController
    {
        public IHttpActionResult Get(int key)
        {
            foreach (var person in AlternateKeysDataSource.People)
            {
                object value;
                if (person.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(person);
                    }
                }
            }

            return NotFound();
        }

        // keyCountry without [FromODataUri]
        // keyPassport with [FromODataUri]
        public IHttpActionResult Get(string keyCountry, [FromODataUri]string keyPassport)
        {
            return FindPeopleByCountryAndPassport(keyCountry, keyPassport);
        }

        [HttpGet]
        [ODataRoute("People(Country={country},Passport={passport})")]
        public IHttpActionResult FindPeopleByCountryAndPassport([FromODataUri]string country, [FromODataUri]string passport)
        {
            foreach (var person in AlternateKeysDataSource.People)
            {
                object value;
                if (person.TryGetPropertyValue("Country", out value))
                {
                    string countryValue = (string)value;
                    if (person.TryGetPropertyValue("Passport", out value))
                    {
                        string passportValue = (string)value;
                        if (countryValue == country && passportValue == passport)
                        {
                            return Ok(person);
                        }
                    }
                }
            }

            return NotFound();
        }
    }

    public class CompaniesController : ODataController
    {
        public IHttpActionResult Get(int key)
        {
            foreach (var company in AlternateKeysDataSource.Companies)
            {
                object value;
                if (company.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(company);
                    }
                }
            }

            return NotFound();
        }

        /* Not supported now: see github issue: https://github.com/OData/odata.net/issues/294
        [HttpGet]
        [ODataRoute("Companies(City={city},Street={street})")]
        public IHttpActionResult GetCompanyByLocation([FromODataUri]string city, [FromODataUri]string street)
        {
            foreach (var company in AlternateKeysDataSource.Companies)
            {
                object value;
                if (company.TryGetPropertyValue("Location", out value))
                {
                    IEdmComplexObject location = value as IEdmComplexObject;
                    if (location == null)
                    {
                        return NotFound();
                    }

                    if (location.TryGetPropertyValue("City", out value))
                    {
                        string locCity = (string) value;

                        if (location.TryGetPropertyValue("Street", out value))
                        {
                            string locStreet = (string) value;
                            if (locCity == city && locStreet == street)
                            {
                                return Ok(company);
                            }
                        }
                    }
                }
            }

            return NotFound();
        }*/
    }
}
