// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public Address HeadQuarterAddress { get; set; }
        [Singleton]
        public Employee CEO { get; set; }
        public int CEOID { get; set; }
        public List<Employee> ComplanyEmployees { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Address> Subsidiaries { get; set; }
    }
}
