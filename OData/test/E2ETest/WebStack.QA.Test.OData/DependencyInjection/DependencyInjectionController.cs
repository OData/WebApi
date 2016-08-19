using System.Collections.Generic;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.DependencyInjection
{
    public class CustomersController : ODataController
    {
        private List<Customer> _customers;

        [EnableQuery]
        public List<Customer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery]
        public Customer Get(int key)
        {
            Generate();
            return _customers[key - 1];
        }

        [HttpGet]
        public CustomerType EnumFunction()
        {
            return CustomerType.Normal;
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            Generate();
            return _customers[key].Orders;
        }

        [EnableQuery]
        public List<Address> GetAddresses(int key)
        {
            List<Address> addresses = new List<Address>();
            for (int i = 1; i < 10; i++)
            {
                var address = new Address
                {
                    Name = "Address" + i
                };

                addresses.Add(address);
            }

            return addresses;
        }

        public void Generate()
        {
            _customers = new List<Customer>();
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Address = new Address
                    {
                        Name = "City" + i,
                        Street = "Street" + i,
                    },
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i * 2 - 1
                        },
                        new Order
                        {
                            Id = i * 2
                        }
                    },
                };

                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : ODataController
    {
        private List<Order> _orders;

        [EnableQuery]
        public List<Order> Get()
        {
            Generate();
            return _orders;
        }

        public void Generate()
        {
            if (_orders == null)
            {
                _orders = new List<Order>();
                for (int i = 1; i < 10; i++)
                {
                    var order = new Order
                    {
                        Id = i,
                        Name = "Order" + i
                    };

                    _orders.Add(order);
                }
            }
        }
    }
}
