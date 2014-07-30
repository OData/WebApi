// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Represents an attribute that can be placed on an action of an <see cref="ODataController"/> to specify
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
    }
}
