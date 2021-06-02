﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert
{
    public class Employee
    {
        [Key]
        public int ID { get; set; }
        public String Name { get; set; }        
        public List<Skill> SkillSet { get; set; }
        public Gender Gender { get; set; }
        public AccessLevel AccessLevel { get; set; }
        
        [AutoExpand]
        public List<Friend> Friends { get; set; }

        public List<NewFriend> NewFriends { get; set; }

        public List<UnTypedFriend> UnTypedFriends { get; set; }

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

    public class Friend
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Range(10, 20)]
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

        [AutoExpand]
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