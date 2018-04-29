// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// NonODataController is an abstracted ASP.NET [Core] controller.
    /// </summary>
    public class TestNonODataController : ApiController
    {
        [NonAction]
        public new TestOkObjectResult<T> Ok<T>(T value) { return new TestOkObjectResult<T>(base.Ok<T>(value)); }
    }
}
