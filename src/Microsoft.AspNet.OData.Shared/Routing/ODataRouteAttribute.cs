//-----------------------------------------------------------------------------
// <copyright file="ODataRouteAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Represents an attribute that can be placed on an action of an ODataController to specify
    /// the OData URLs that the action handles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ODataRouteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteAttribute"/> class.
        /// </summary>
        public ODataRouteAttribute()
            : this(pathTemplate: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteAttribute"/> class.
        /// </summary>
        /// <param name="pathTemplate">The OData URL path template that this action handles.</param>
        public ODataRouteAttribute(string pathTemplate)
        {
            PathTemplate = pathTemplate ?? String.Empty;
        }

        /// <summary>
        /// Gets the OData URL path template that this action handles.
        /// </summary>
        public string PathTemplate { get; private set; }

        /// <summary>
        /// Gets or sets the OData route with which to associate the attribute.
        /// </summary>
        public string RouteName { get; set; }
    }
}
