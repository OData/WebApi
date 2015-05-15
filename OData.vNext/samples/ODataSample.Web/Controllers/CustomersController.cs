using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
    [Route("api/Customers")]
    public class CustomersController : Controller
    {
        private readonly SampleContext _sampleContext;

        public CustomersController(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        // GET: api/Customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return _sampleContext.Customers;
        }

        // GET api/Customers/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Json(_sampleContext.Customers.Single(p => p.CustomerId == id));
        }

        // POST api/Customers
        [HttpPost]
        public IActionResult Post([FromBody]Customer value)
        {
            var locationUri = $"http://localhost:9091/api/Customers/{value.CustomerId}";
            return Created(locationUri, _sampleContext.AddCustomer(value));
        }

        // PUT api/Customers/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Customer value)
        {
            if (!_sampleContext.UpdateCustomer(id, value))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_sampleContext.DeleteCustomer(id))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }
    }
}
