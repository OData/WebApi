//-----------------------------------------------------------------------------
// <copyright file="Company.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
