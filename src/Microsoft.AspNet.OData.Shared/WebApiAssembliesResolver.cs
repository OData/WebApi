//-----------------------------------------------------------------------------
// <copyright file="WebApiAssembliesResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi assembly resolver to OData WebApi.
    /// </summary>
    internal partial class WebApiAssembliesResolver
    {
        /// <summary>
        /// This static instance is used in the shared code in places where the request container context
        /// is not known or does not contain an instance of IWebApiAssembliesResolver.
        /// </summary>
        public static IWebApiAssembliesResolver Default = new WebApiAssembliesResolver();
    }
}
