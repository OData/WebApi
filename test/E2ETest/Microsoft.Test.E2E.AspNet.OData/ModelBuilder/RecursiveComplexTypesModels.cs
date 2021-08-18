//-----------------------------------------------------------------------------
// <copyright file="RecursiveComplexTypesModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive
{
    // Scenario 1: Direct reference (complex type points to itself)
    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string CountryOrRegion { get; set; }

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
