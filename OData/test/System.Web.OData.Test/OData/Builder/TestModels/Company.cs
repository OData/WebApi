// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Builder.TestModels
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public Address HeadQuarterAddress { get; set; }
        [Singleton]
        public Employee CEO { get; set; }
        public List<Employee> ComplanyEmployees { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Address> Subsidiaries { get; set; }
    }
}
