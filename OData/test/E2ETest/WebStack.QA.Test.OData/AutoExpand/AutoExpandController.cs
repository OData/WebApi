using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.AutoExpand
{
    public class CustomersController : ODataController
    {
        private readonly AutoExpandContext _db = new AutoExpandContext();

        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AutoExpandContext();
            return db.Customers;
        }

        [EnableQuery]
        public SingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new AutoExpandContext();
            return SingleResult.Create(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            Customer previousCustomer = null;
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Order = new Order
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Ammount = i * 1000
                        }
                    },
                };

                if (i > 1)
                {
                    customer.Friend = previousCustomer;
                }

                _db.Customers.Add(customer);
                previousCustomer = customer;
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

    public class PeopleController : ODataController
    {
        private readonly AutoExpandContext _db = new AutoExpandContext();

        [EnableQuery(MaxExpansionDepth = 4)]
        public IQueryable<People> Get()
        {
            ResetDataSource();
            var db = new AutoExpandContext();
            return db.People;
        }

        public void Generate()
        {
            People previousPeople = null;
            for (int i = 1; i < 10; i++)
            {
                var people = new People
                {
                    Id = i,
                    Order = new Order
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Ammount = i * 1000
                        }
                    },
                };

                if (i > 1)
                {
                    people.Friend = previousPeople;
                }

                _db.People.Add(people);
                previousPeople = people;
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
