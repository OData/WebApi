// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Test.Common
{
    /// <summary>
    /// A generic controller base which derives from a platform-specific type.
    /// </summary>
#if NETCORE
    public class TestControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
    }
#else
    public class TestControllerBase : System.Web.Http.ApiController
    {
    }
#endif
}
