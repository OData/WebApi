// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.AspNet.OData
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
