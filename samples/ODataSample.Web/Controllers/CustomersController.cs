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
        public IActionResult Get(int id)
        {
            var customer = _sampleContext.FindCustomer(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer);
        }

        [HttpGet("{id}/FirstName")]
        public IActionResult GetFirstName(int id)
        {
            var customer = _sampleContext.FindCustomer(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.FirstName);
        }

        [HttpGet("{id}/LastName")]
        public IActionResult GetLastName(int id)
        {
            var customer = _sampleContext.FindCustomer(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.LastName);
        }

        [HttpGet("{id}/CustomerId")]
        public IActionResult GetCustomerId(int id)
        {
            var customer = _sampleContext.FindCustomer(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(customer.CustomerId);
        }

        [HttpGet("{id}/Products")]
        public IActionResult GetProducts(int id)
        {
            var customer = _sampleContext.FindCustomer(id);
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
