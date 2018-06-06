// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive
{
    public class RecursiveComplexTypesTests_UserEntity
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Customer Customer { get; set; }

        public RecursiveComplexTypesTests_Address Address { get; set; }

        public RecursiveComplexTypesTests_Directory HomeDirectory { get; set; }

        public List<RecursiveComplexTypesTests_Field> CustomFields { get; set; }

        public RecursiveComplexTypesTests_Base Base { get; set; }
    }

    public class RecursiveComplexTypesTests_JustCustomer
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Customer Customer { get; set; }
    }

    public class RecursiveComplexTypesTests_JustAddress
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Address Address { get; set; }
    }

    public class RecursiveComplexTypesTests_JustHomeDirectory
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Directory HomeDirectory { get; set; }
    }

    public class RecursiveComplexTypesTests_JustCustomFields
    {
        public int ID { get; set; }

        public List<RecursiveComplexTypesTests_Field> CustomFields { get; set; }
    }

    public class RecursiveComplexTypesTests_JustBase
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Base Base { get; set; }
    }

    public class RecursiveComplexTypesTests_JustDerived
    {
        public int ID { get; set; }

        public RecursiveComplexTypesTests_Derived Derived { get; set; }
    }

    // Scenario 1: Direct reference (complex type points to itself)
    public class RecursiveComplexTypesTests_Address
    {
        public string Street { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public RecursiveComplexTypesTests_Address PreviousAddress { get; set; }
    }

    // Scenario 2: Collection (complex type points to itself via a collection)
    public class RecursiveComplexTypesTests_Field
    {
        public string Name { get; set; }

        public string DataType { get; set; }

        public List<RecursiveComplexTypesTests_Field> SubFields { get; set; }
    }

    // Scenario 3: Composition (complex type points to itself via indirect recursion)
    public class RecursiveComplexTypesTests_Customer
    {
        public string Name { get; set; }

        public List<RecursiveComplexTypesTests_Account> Accounts { get; set; }
    }

    public class RecursiveComplexTypesTests_Account
    {
        public int Number { get; set; }

        public RecursiveComplexTypesTests_Customer Owner { get; set; }
    }

    // Scenario 4: Inheritance (complex type has sub-type that points back to the base type via a collection)
    public class RecursiveComplexTypesTests_File
    {
        public string Name { get; set; }

        public int Size { get; set; }
    }

    public class RecursiveComplexTypesTests_Directory : RecursiveComplexTypesTests_File
    {
        public List<RecursiveComplexTypesTests_File> Files { get; set; }
    }

    // Scenario 5: Hybrid of mutual recursion and inheritance.
    public class RecursiveComplexTypesTests_Base
    {
        public RecursiveComplexTypesTests_Derived Derived { get; set; }
    }

    public class RecursiveComplexTypesTests_Derived : RecursiveComplexTypesTests_Base
    {
        public RecursiveComplexTypesTests_Base Base { get; set; }
    }
}

