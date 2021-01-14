// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.E2E.AspNet.OData.QueryValidationBeforeAction
{
    public class CustomersController : Controller
    {
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Top, MaxTop = 10)]
        public IEnumerable<Customer> GetCustomers()
        {
            throw new Exception("Controller should never be invoked as query validation should fail");
        }
    }
}
