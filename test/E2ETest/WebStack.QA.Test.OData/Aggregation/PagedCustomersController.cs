// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData;

namespace WebStack.QA.Test.OData.Aggregation.Paged
{
    public class CustomersController : BaseCustomersController
    {
        [EnableQuery(PageSize = 5)]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers;
        }
    }
}
