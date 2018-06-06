// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive
{
    public class UserEntity
    {
        public int ID { get; set; }

        public Customer Customer { get; set; }

        public Address Address { get; set; }

        public Directory HomeDirectory { get; set; }

        public List<Field> CustomFields { get; set; }

        public Base Base { get; set; }
    }

    public class JustCustomer
    {
        public int ID { get; set; }

        public Customer Customer { get; set; }
    }

    public class JustAddress
    {
        public int ID { get; set; }

        public Address Address { get; set; }
    }

    public class JustHomeDirectory
    {
        public int ID { get; set; }

        public Directory HomeDirectory { get; set; }
    }

    public class JustCustomFields
    {
        public int ID { get; set; }

        public List<Field> CustomFields { get; set; }
    }

    public class JustBase
    {
        public int ID { get; set; }

        public Base Base { get; set; }
    }

    public class JustDerived
    {
        public int ID { get; set; }

        public Derived Derived { get; set; }
    }

    // Scenario 1: Direct reference (complex type points to itself)
    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public Address PreviousAddress { get; set; }
    }

    // Scenario 2: Collection (complex type points to itself via a collection)
    public class Field
    {
        public string Name { get; set; }

        public string DataType { get; set; }

        public List<Field> SubFields { get; set; }
    }

    // Scenario 3: Composition (complex type points to itself via indirect recursion)
    public class Customer
    {
        public string Name { get; set; }

        public List<Account> Accounts { get; set; }
    }

    public class Account
    {
        public int Number { get; set; }

        public Customer Owner { get; set; }
    }

    // Scenario 4: Inheritance (complex type has sub-type that points back to the base type via a collection)
    public class File
    {
        public string Name { get; set; }

        public int Size { get; set; }
    }

    public class Directory : File
    {
        public List<File> Files { get; set; }
    }

    // Scenario 5: Hybrid of mutual recursion and inheritance.
    public class Base
    {
        public Derived Derived { get; set; }
    }

    public class Derived : Base
    {
        public Base Base { get; set; }
    }
}

