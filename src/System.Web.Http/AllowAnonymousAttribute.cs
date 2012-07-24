// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// Actions and controllers marked with this attribute are skipped by <see cref="AuthorizeAttribute"/> during authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
    }
}
