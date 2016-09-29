// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Represents an attribute that can be placed on a controller or an action to specify
    /// the OData URLs that the action handles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
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
    }
}
