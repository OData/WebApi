//-----------------------------------------------------------------------------
// <copyright file="BulkInsertDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert
{
    [AutoExpand]
    public class Employee
    {
        [Key]
        public int ID { get; set; }
        public String Name { get; set; }        
        public List<Skill> SkillSet { get; set; }
        public Gender Gender { get; set; }
        public AccessLevel AccessLevel { get; set; }
        
        public List<Friend> Friends { get; set; }

        public List<NewFriend> NewFriends { get; set; }

        public List<UnTypedFriend> UnTypedFriends { get; set; }

        public FavoriteSports FavoriteSports { get; set; }

        public IODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
    }

    [Flags]
    public enum AccessLevel
    {
        Read = 1,
        Write = 2,
        Execute = 4
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum Skill
    {
        CSharp,
        Sql,
        Web,
    }

    public enum Sport
    {
        Pingpong,
        Basketball
    }

    public class FavoriteSports
    {
        public string Sport { get; set; }
    }

    public class Friend
    {
        [Key]
        public int Id { get; set; }
    
        public string Name { get; set; }
       
        public int Age { get; set; }

        public List<Order> Orders { get; set; }

    }


    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int Price { get; set; }
    }

    public class NewFriend
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
        public IODataInstanceAnnotationContainer InstanceAnnotations { get; set; }

        [Contained]
        public List<NewOrder> NewOrders { get; set; }

    }

    public class MyNewFriend: NewFriend
    {
        public string MyName { get; set; }

        [Contained]
        public List<MyNewOrder> MyNewOrders { get; set; }
    }

    public class MyNewOrder
    {
        [Key]
        public int Id { get; set; }

        public int Price { get; set; }

        public int Quantity { get; set; }

        public IODataIdContainer Container { get; set; }
    }

    public class NewOrder
    {
        [Key]
        public int Id { get; set; }

        public int Price { get; set; }

        public int Quantity { get; set; }

        public IODataIdContainer Container {get;set;}
    }


    public class Company
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public List<NewOrder> OverdueOrders { get; set; }

        public List<MyNewOrder> MyOverdueOrders { get; set; }
    }

    public class UnTypedEmployee
    {
        [Key]
        public int ID { get; set; }
        public String Name { get; set; }
       
        public List<UnTypedFriend> UnTypedFriends { get; set; }
    }

    public class UnTypedFriend
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
               
        public UnTypedAddress Address { get; set; }

        public IODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
    }

    public class UnTypedAddress
    {
        [Key]
        public int Id { get; set; }

        public string Street { get; set; }
    }

    public class FriendColl<T> : ICollection<T>
    {
        public FriendColl() { _items = new List<T>(); }

        private IList<T> _items;

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void Add(T item)
        {
            var _item = item as NewFriend;
            if (_item != null && _item.Age < 10)
            {
                throw new NotImplementedException();
            }

            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();

            //return _items.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }

}