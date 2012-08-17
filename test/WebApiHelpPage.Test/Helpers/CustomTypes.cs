// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace WebApiHelpPageWebHost.UnitTest
{
    public class MyGenericType<T, T2>
    {
        public T MyProperty { get; set; }
        public T2 MyProperty2 { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public Dictionary<Item, string> Items { get; set; }
        public DateTime ShipDate { get; set; }
    }

    public class Item
    {
        public Customer Buyer { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class ComplexTypeWithPublicFields
    {
        public string Name;
        public Guid Id;
        public Item item;
    }

    public struct ComplexStruct
    {
        public string Name;
        public Guid Id;
        public DateTime Time { get; set; }
        public DateTimeKind Kind { get; set; }
    }

    public class TypeWithNoDefaultConstructor
    {
        public TypeWithNoDefaultConstructor(int id, string name) { }
    }

    internal class NonPublicType { }

    public enum EmptyEnum { }
}
