// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.OData.Builder.TestModels
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public Decimal BaseSalary { get; set; }
        public DateTimeOffset Birthday { get; set; }
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
