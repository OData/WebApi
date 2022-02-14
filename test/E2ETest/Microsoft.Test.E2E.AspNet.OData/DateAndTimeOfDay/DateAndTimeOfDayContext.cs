//-----------------------------------------------------------------------------
// <copyright file="DateAndTimeOfDayContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Data.Entity;

namespace Microsoft.Test.E2E.AspNet.OData.DateAndTimeOfDay
{
    public class DateAndTimeOfDayContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=DateAndTimeOfDayEfDbContext1";

        public DateAndTimeOfDayContext()
            : base(ConnectionString)
        {
        }

        public DbSet<EfCustomer> Customers { get; set; }
    }

    public class EdmDateWithEfContext : DbContext
    {
        public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EdmDateWithEfDbContext1";

        public EdmDateWithEfContext()
            : base(ConnectionString)
        {
        }

        public IDbSet<EfPerson> People { get; set; }
    }
}
