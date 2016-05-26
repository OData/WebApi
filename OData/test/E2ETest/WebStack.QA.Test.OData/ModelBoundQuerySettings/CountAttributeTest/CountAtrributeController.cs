using System.Collections.Generic;
using System.Linq;
using System.Web.OData;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.CountAttributeTest
{
    public class CustomersController : ODataController
    {
        private List<Customer> _customers;

        [EnableQuery(MaxExpansionDepth = 10)]
        public List<Customer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            List<Order> orders = new List<Order>();
            for (int i = 1; i < 10; i++)
            {
                var order = new Order
                {
                    Id = i,
                    Name = "Order" + i,
                };

                orders.Add(order);
            }

            return orders;
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
                    CountableOrders = new List<Order>
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
        
        [EnableQuery(MaxExpansionDepth = 6)]
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
