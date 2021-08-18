//-----------------------------------------------------------------------------
// <copyright file="CustomerControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Test.Common.Models;

namespace Microsoft.AspNet.OData.Test.Query.Controllers
{
    public class CustomerHighLevelController : ODataController
    {
        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public object GetObject()
        {
            // this can return Customer or BellevueCustomer
            throw new NotImplementedException();
        }

        [EnableQuery]
        public IEnumerable<BellevueCustomer> GetIEnumerableOfCustomer()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public Collection<SeattleCustomer> GetCollectionOfCustomer()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public List<RedmondCustomer> GetListOfCustomer()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public CustomerCollection GetStronglyTypedCustomer()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public Customer[] GetArrayOfCustomers()
        {
            throw new NotImplementedException();
        }

        [EnableQuery]
        public NonGenericEnumerable GetNonGenericEnumerable()
        {
            throw new NotImplementedException();
        }

        public IEnumerable GetNonQueryable()
        {
            throw new NotImplementedException();
        }

        public TwoGenericsCollection GetTwoGenericsCollection()
        {
            throw new NotImplementedException();
        }
    }

    public class CustomerCollection : IEnumerable<Customer>
    {
        List<Customer> _list;

        public CustomerCollection()
        {
            _list = new List<Customer>()
            {
                new Customer(){ Name = "D" },
                new Customer(){ Name = "E" },
                new Customer(){ Name = "F" },
            };
        }

        public IEnumerator<Customer> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }

    public class TwoGenericsCollection : IMyInterface<Customer, RentonCustomer>
    {
    }

    public interface IMyInterface<T1, T2>
    {
    }
}
