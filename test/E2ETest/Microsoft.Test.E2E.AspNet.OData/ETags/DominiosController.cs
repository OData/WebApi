// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.AspNet.OData;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class DominiosController : ODataController
    {
        private ETagCurrencyTokenEfContext _db = new ETagCurrencyTokenEfContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_db.Dominios);
        }
    }
}
