using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.SingleResultTest
{
    public class CustomersController : ODataController
    {
        private readonly SingleResultContext _db = new SingleResultContext();

        [EnableQuery]
        public SingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new SingleResultContext();
            return SingleResult.Create(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i,
                        }
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
        }
    }
}
