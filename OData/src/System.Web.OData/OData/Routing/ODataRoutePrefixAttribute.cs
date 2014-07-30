// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Represents an attribute that can be placed on an OData controller to specify
    /// the prefix that will be used for all actions of that controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ODataRoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteAttribute"/> class.
        /// </summary>
        /// <param name="prefix">The OData URL path template that this action handles.</param>
        public ODataRoutePrefixAttribute(string prefix)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                throw Error.ArgumentNullOrEmpty("prefix");
            }

            Prefix = prefix;
        }

        /// <summary>
        /// Gets the OData URL path template that this action handles.
        /// </summary>
        public string Prefix { get; private set; }
    }
}
