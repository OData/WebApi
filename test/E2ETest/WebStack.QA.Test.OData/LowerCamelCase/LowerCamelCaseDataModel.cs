// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebStack.QA.Test.OData.LowerCamelCase
{
    [DataContract]
    public class Employee
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember(Name = "Name")]
        public string FullName { get; set; }

        [DataMember]
        public Gender Sex { get; set; }

        [DataMember]
        public Address Address { get; set; }

        [DataMember]
        public Employee Next { get; set; }

        [DataMember]
        public Manager Manager { get; set; }
    }

    [DataContract]
    public class Manager : Employee
    {
        [DataMember]
        public IList<Employee> DirectReports { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }
}
