//-----------------------------------------------------------------------------
// <copyright file="ODataOperationImportController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
