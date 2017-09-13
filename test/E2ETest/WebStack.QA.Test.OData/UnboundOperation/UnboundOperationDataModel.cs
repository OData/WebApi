// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace WebStack.QA.Test.OData.UnboundOperation
{
    public class ConventionCustomer
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ConventionAddress Address { get; set; }
        public List<ConventionOrder> Orders { get; set; }
    }

    public class ConventionAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class ConventionOrder
    {
        public int ID { get; set; }
        public string OrderName { get; set; }
        public decimal Price { get; set; }
        public Guid OrderGuid { get; set; }
    }

    public class ConventionPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool Male { get; set; }
    }

    public enum ConventionGender
    {
        Male,
        Female,
    }
}
