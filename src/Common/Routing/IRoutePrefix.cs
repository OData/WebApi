// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Defines a route prefix.</summary>
    public interface IRoutePrefix
    {
        /// <summary>Gets the route prefix.</summary>
        string Prefix { get; }
    }
}