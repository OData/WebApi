//-----------------------------------------------------------------------------
// <copyright file="BoundOperationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.BoundOperation
{
    public class Employee
    {
        public Employee()
        {
            OptionalAddresses = new List<Address>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public IList<string> Emails { get; set; }
        public int Salary { get; set; }
        public IList<Address> OptionalAddresses { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    public class Manager : Employee
    {
        public int Heads { get; set; }
    }

    public class SubAddress : Address
    {
        public double Code { get; set; }
    }

    public enum Color
    {
        Red,
        Blue,
        Green
    }
}
