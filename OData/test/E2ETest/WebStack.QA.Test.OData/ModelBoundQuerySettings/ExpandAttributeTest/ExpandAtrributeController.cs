using System.Collections.Generic;
using System.Linq;
using System.Web.OData;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.ExpandAttributeTest
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

        public void Generate()
        {
            _customers = new List<Customer>();
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i,
                        Price = i * 100
                    },
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
                    AutoExpandOrder = new Order
                    {
                        Id = i,
                        Name = "AutoExpandOrder" + i
                    }
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
                        Name = "Order" + i,
                        Customers = new List<Customer>
                        {
                            new Customer
                            {
                                Id = i,
                                Name = "Customer" + i
                            }
                        },
                        Customers2 = new List<Customer>
                        {
                            new Customer
                            {
                                Id = i,
                                Name = "Customer" + i
                            }
                        }
                    };

                    _orders.Add(order);
                }
            }
        }
    }
}
