//-----------------------------------------------------------------------------
// <copyright file="TestControllerBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
