#if !NETCOREAPP3_0
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class LinqToSqlDatabaseContext :DataContext
    {
        public LinqToSqlDatabaseContext() :
            base("Data Source=.;Initial Catalog=L2S;Integrated Security=true")
        {
            this.Customers = GetTable<Customer>();
        }

        public Table<Customer> Customers { get; set; }
    }
}
#endif