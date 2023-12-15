//-----------------------------------------------------------------------------
// <copyright file="MediaTypesOrdersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.MediaTypes
{
    public class MediaTypesOrdersController : TestODataController
    {
        private List<MediaTypesOrder> orders = new List<MediaTypesOrder>
        {
            new MediaTypesOrder { Id = 1, Amount = 130, TrackingNumber = 9223372036854775807L }
        };

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(orders);
        }

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            var order = orders.FirstOrDefault(c => c.Id == key);

            if (order == null)
            {
                throw new ArgumentOutOfRangeException("key");
            }

            return Ok(order);
        }
    }
}
