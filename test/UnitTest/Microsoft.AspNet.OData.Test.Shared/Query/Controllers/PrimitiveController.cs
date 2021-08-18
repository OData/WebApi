//-----------------------------------------------------------------------------
// <copyright file="PrimitiveController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;

namespace Microsoft.AspNet.OData.Test.Query.Controllers
{
    public class PrimitiveController : ODataController
    {
        [EnableQuery]
        public IQueryable<int> Get()
        {
            return GetIEnumerableOfInt().AsQueryable<int>();
        }

        [EnableQuery]
        public IEnumerable<int> GetIEnumerableOfInt()
        {
            return new List<int>() { 1, 2, 3, 4, 5 };
        }
    }
}
