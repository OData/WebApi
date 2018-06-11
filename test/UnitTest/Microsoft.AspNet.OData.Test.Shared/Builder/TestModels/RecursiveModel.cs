// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class RecursivePropertyContainer
    {
        public Guid Id { get; set; }

        public GenericSurrogate Item { get; set; }
    }

    public abstract class GenericSurrogate
    {
    }

    public class MyExpression : GenericSurrogate
    {
        public GenericSurrogate Item { get; set; }
    }

    public class Parent
    {
        public Child OnlyChild { get; set; }
    }

    public class Child : Parent
    {
        public Parent FavoriteParent { get; set; }

        public Car Car { get; set; }
    }

    public class Person
    {
        [Key]
        public string Name { get; set; }

        public Parent ResponsibleGuardian { get; set; }
    }

    public class CountryDetails
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
    }

    public class RecursiveAddress
    {
        public string Street { get; set; }

        public string City { get; set; }

        // Navigation property
        public CountryDetails Country { get; set; }

        // Recursive reference
        public RecursiveAddress PreviousAddress { get; set; }
    }

    public class Organization
    {
        [Key]
        public string Name { get; set; }

        public RecursiveAddress HeadquartersAddress { get; set; }
    }
}
