// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.OData.Query.Controllers
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
