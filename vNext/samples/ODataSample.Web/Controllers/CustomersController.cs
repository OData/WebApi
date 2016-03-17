namespace ODataSample.Web.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData;
    using ODataSample.Web.Models;

    [EnableQuery]
    [Route("odata/Customers")]
    public class CustomersController : Controller
    {
        private readonly SampleContext _sampleContext;

        public CustomersController(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        // GET: odata/Customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return _sampleContext.Customers;
        }

        // GET odata/Customers(5)
        [HttpGet("{customerId}")]
        public IActionResult Get(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        // GET odata//FindCustomersWithProduct(productId=1)
        [HttpGet("FindCustomersWithProduct(ProductId={productId})")]
        public IActionResult FindCustomersWithProduct(int productId)
        {
            var customer = _sampleContext.FindCustomersWithProduct(productId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        // PUT odata//FindCustomersWithProduct(productId=1)
        [HttpPut("{customerId}/AddCustomerProduct(ProductId={productId})")]
        public IActionResult AddCustomerProduct(int customerId, [FromBody] int productId)
        {
            var customer = _sampleContext.AddCustomerProduct(customerId, productId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }

        // PUT odata//FindCustomersWithProduct(productId=1)
        [HttpPut("{customerId}/AddCustomerProducts(ProductId={productId})")]
        public IActionResult AddCustomerProducts(int customerId, [FromBody] List<int> products)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            foreach (var productId in products)
            {
                _sampleContext.AddCustomerProduct(customerId, productId);
            }

            return new ObjectResult(customer);
        }

        // GET odata/Customers(1)/FirstName
        [HttpGet("{customerId}/FirstName")]
        public IActionResult GetFirstName(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer.FirstName);
        }

        // GET odata/Customers(1)/LastName
        [HttpGet("{customerId}/LastName")]
        public IActionResult GetLastName(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer.LastName);
        }

        // GET odata/Customers(1)/CustomerId
        [HttpGet("{customerId}/CustomerId")]
        public IActionResult GetCustomerId(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer.CustomerId);
        }

        // GET odata/Customers(1)/Products
        [HttpGet("{customerId}/Products")]
        public IActionResult GetProducts(int customerId)
        {
            var customer = _sampleContext.FindCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer.Products);
        }

        // POST odata/Customers
        [HttpPost]
        public IActionResult Post([FromBody]Customer value)
        {
            var locationUri = $"http://localhost:5000/odata/Customers/{value.CustomerId}";
            return Created(locationUri, _sampleContext.AddCustomer(value));
        }

        // PUT odata/Customers/5
        [HttpPut("{customerId}")]
        public IActionResult Put(int customerId, [FromBody]Customer value)
        {
            if (!_sampleContext.UpdateCustomer(customerId, value))
            {
                return NotFound();
            }

            return new NoContentResult();
        }

        // DELETE odata/Customers/5
        [HttpDelete("{customerId}")]
        public IActionResult Delete(int customerId)
        {
            if (!_sampleContext.DeleteCustomer(customerId))
            {
                return NotFound();
            }

            return new NoContentResult();
        }
    }
}
