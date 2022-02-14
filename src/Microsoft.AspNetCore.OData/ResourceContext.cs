//-----------------------------------------------------------------------------
// <copyright file="ResourceContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An instance of <see cref="ResourceContext"/> gets passed to the self link (
    /// <see cref="M:NavigationSourceConfiguration.HasIdLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasEditLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasReadLink"/>
    /// ) and navigation link (
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertyLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertiesLink"/>
    /// ) builders and can be used by the link builders to generate links.
    /// </summary>
    public partial class ResourceContext
    {
        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public HttpRequest Request
        {
            get
            {
                return SerializerContext.Request;
            }
            set
            {
                SerializerContext.Request = value;
            }
        }
    }
}
