using Microsoft.AspNet.Mvc;
using ODataSample.Web.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;

namespace ODataSample.Web.Controllers
{
    [EnableQuery]
    [Route("odata/Customers")]
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
        public IActionResult Get(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer);
        }

        [HttpGet("{id}/FirstName")]
        public IActionResult GetFirstName(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.FirstName);
        }

        [HttpGet("{id}/LastName")]
        public IActionResult GetLastName(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.LastName);
        }

        [HttpGet("{id}/CustomerId")]
        public IActionResult GetCustomerId(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.CustomerId);
        }

        [HttpGet("{id}/Products")]
        public IActionResult GetProducts(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.Products);
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
        public IActionResult Put(int customerId, [FromBody]Customer value)
        {
            if (!_sampleContext.UpdateCustomer(customerId, value))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int customerId)
        {
            if (!_sampleContext.DeleteCustomer(customerId))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }
    }
}
