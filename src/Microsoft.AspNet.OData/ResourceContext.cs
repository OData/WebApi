//-----------------------------------------------------------------------------
// <copyright file="ResourceContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Web.Http.Routing;

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
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestMessage Request
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

        /// <summary>
        /// Gets or sets a <see cref="UrlHelper"/> that may be used to generate links while serializing this resource
        /// instance.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public UrlHelper Url
        {
            get
            {
                return SerializerContext.Url;
            }
            set
            {
                SerializerContext.Url = value;
            }
        }
    }
}
