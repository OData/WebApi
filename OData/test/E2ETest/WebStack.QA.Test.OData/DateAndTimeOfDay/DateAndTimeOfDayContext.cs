using System.Data.Entity;

namespace WebStack.QA.Test.OData.DateAndTimeOfDay
{
    public class DateAndTimeOfDayContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=DateAndTimeOfDayEfDbContext";

        public DateAndTimeOfDayContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EfCustomer> Customers { get; set; }
    }

    public class EdmDateWithEfContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\v11.0;Integrated Security=True;Initial Catalog=EdmDateWithEfDbContext";

        public EdmDateWithEfContext()
            : base(ConnectionString)
        {
        }

        public IDbSet<EfPerson> People { get; set; }
    }
}
