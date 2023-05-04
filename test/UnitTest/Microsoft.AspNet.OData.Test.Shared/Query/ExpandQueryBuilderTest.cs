//-----------------------------------------------------------------------------
// <copyright file="ExpandQueryBuilderTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ExpandQueryBuilderTest
    {
        IEdmModel model;
        ExpandQueryBuilder expandQueryBuilder;

        public ExpandQueryBuilderTest()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Employee> employee = builder.EntitySet<Employee>("Employees");
            EntitySetConfiguration<Friend> friend = builder.EntitySet<Friend>("Friends");
            EntitySetConfiguration<Order> order = builder.EntitySet<Order>("Orders");
            EntitySetConfiguration<NewFriend> newfriend = builder.EntitySet<NewFriend>("NewFriends");

            model = builder.GetEdmModel();

            expandQueryBuilder = new ExpandQueryBuilder();
        }

        [Fact]
        public void SimpleOneLevelExpand()
        {
            Employee employee = new Employee()
            {
                ID = 1,
                Friends = new List<Friend> { new Friend() { Id = 1001 } }
            };

            string expected = "$expand=Friends";
            string actual = expandQueryBuilder.GenerateExpandQueryParameter(employee, model);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TwoLevelsExpand()
        {
            Employee employee = new Employee()
            {
                ID = 1,
                Friends = new List<Friend> { new Friend() { Id = 1001, Orders = new List<Order> { new Order { Id = 10001 } } } }
            };

            string expected = "$expand=Friends($expand=Orders)";
            string actual = expandQueryBuilder.GenerateExpandQueryParameter(employee, model);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MultipleTwoLevelsExpand()
        {
            Employee employee = new Employee()
            {
                ID = 1,
                Friends = new List<Friend> { new Friend() { Id = 1001, Orders = new List<Order> { new Order { Id = 10001 } } } },
                NewFriends = new List<NewFriend> { new NewFriend() { Id = 1001, NewOrders = new List<NewOrder> { new NewOrder { Id = 10001 } } } }
            };

            string expected = "$expand=Friends($expand=Orders),NewFriends($expand=NewOrders)";
            string actual = expandQueryBuilder.GenerateExpandQueryParameter(employee, model);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MultipleUnevenLevelsExpand()
        {
            Employee employee = new Employee()
            {
                ID = 1,
                Friends = new List<Friend> { new Friend() { Id = 1001, Orders = new List<Order> { new Order { Id = 10001 } } }, new Friend() { Id = 1002, Orders = new List<Order> { new Order { Id = 10002, OrderLines = new List<OrderLine> { new OrderLine { Id = 222 } } }, new Order { Id = 10003 } } } },
                NewFriends = new List<NewFriend> { new NewFriend() { Id = 1001, NewOrders = new List<NewOrder> { new NewOrder { Id = 10001 } } } }
            };

            string expected = "$expand=Friends($expand=Orders($expand=OrderLines)),NewFriends($expand=NewOrders)";
            string actual = expandQueryBuilder.GenerateExpandQueryParameter(employee, model);

            Assert.Equal(expected, actual);
        }

        private class Employee
        {
            [Key]
            public int ID { get; set; }
            public String Name { get; set; }
            public List<Friend> Friends { get; set; }
            public List<NewFriend> NewFriends { get; set; }
        }

        private class Friend
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public List<Order> Orders { get; set; }
        }

        private class Order
        {
            [Key]
            public int Id { get; set; }
            public int Price { get; set; }
            public List<OrderLine> OrderLines { get; set; }
        }

        private class OrderLine
        {
            [Key]
            public int Id { get; set; }
            public int Price { get; set; }
        }

        private class NewFriend
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            [Contained]
            public List<NewOrder> NewOrders { get; set; }
        }

        private class NewOrder
        {
            [Key]
            public int Id { get; set; }
            public int Price { get; set; }
            public int Quantity { get; set; }
            public ODataIdContainer Container { get; set; }
        }
    }
}
