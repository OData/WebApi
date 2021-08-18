//-----------------------------------------------------------------------------
// <copyright file="Employee.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public Decimal BaseSalary { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public IList<Company> IsCeoOf { get; set; }
        public int WorkCompanyId { get; set; }
        public Company WorkCompany { get; set; }
        [Singleton]
        public Employee Boss { get; set; }
        public Address HomeAddress { get; set; }
        public IList<Employee> DirectReports { get; set; }
    }

    public class Manager : Employee
    {
        public Decimal ExtraDraw;
        public Address ExtraOffice { get; set; }
    }

    public class Engineer : Employee
    {
        public int Level { get; set; }
        public Decimal YearEndBonus { get; set; }
    }

    public class SalesPerson : Employee
    {
        public Decimal Bonus { get; set; }
        public IList<Customer> Customers { get; set; }
    }
}
