//-----------------------------------------------------------------------------
// <copyright file="FuzzingController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Fuzzing
{
    public class FuzzingController : TestNonODataController
    {
        private static EntityTypeModel1[] cachedEntities = null;

        static FuzzingController()
        {
            cachedEntities = FuzzingDataInitializer.Generate().ToArray();
        }

        [EnableQuery(PageSize = 999999)]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return cachedEntities;
        }
    }

    public class FuzzingDbController : TestNonODataController, IDisposable
    {
        private FuzzingContext context = new FuzzingContext();

        [EnableQuery]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return context.EntityTypeModel1Set.AsEnumerable();
        }

#if NETFX // IDisposable is only implemented in the AspNet version.
        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }
#else
        public void Dispose()
        {
            context.Dispose();
        }
#endif
    }
}
