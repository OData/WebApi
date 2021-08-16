//-----------------------------------------------------------------------------
// <copyright file="TestNonODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// NonODataController is an abstracted ASP.NET [Core] controller.
    /// </summary>
    public class TestNonODataController : Controller
    {
        [NonAction]
        public new TestOkObjectResult Ok(object value) { return new TestOkObjectResult(value); }
    }
}
