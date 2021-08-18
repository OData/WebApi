//-----------------------------------------------------------------------------
// <copyright file="DominiosController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class DominiosController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private ETagCurrencyTokenEfContext _db = new ETagCurrencyTokenEfContext();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(_db.Dominios);
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }
}
