// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    /// <summary>
    /// Apply this attribute on a method so that it is not publicly reachable via routing. 
    /// The method will not be considered an action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonActionAttribute : Attribute
    {
    }
}
