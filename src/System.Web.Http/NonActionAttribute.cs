// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonActionAttribute : Attribute, IActionMethodSelector
    {
        bool IActionMethodSelector.IsValidForRequest(HttpControllerContext controllerContext, MethodInfo methodInfo)
        {
            return false;
        }
    }
}
