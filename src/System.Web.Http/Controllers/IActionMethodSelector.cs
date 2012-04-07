// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.Controllers
{
    internal interface IActionMethodSelector
    {
        bool IsValidForRequest(HttpControllerContext controllerContext, MethodInfo methodInfo);
    }
}
