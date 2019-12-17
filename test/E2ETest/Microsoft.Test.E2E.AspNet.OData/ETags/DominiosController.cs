// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
