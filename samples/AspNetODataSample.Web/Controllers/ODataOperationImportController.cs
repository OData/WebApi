// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using AspNetODataSample.Web.Models;
using Microsoft.AspNet.OData;

namespace AspNetODataSample.Web.Controllers
{
    public class ODataOperationImportController : ODataController
    {
        [HttpGet]
        public IHttpActionResult RateByOrder(int order)
        {
            return Ok($"In RateByOrder using Order = {order}");
        }
    }
}