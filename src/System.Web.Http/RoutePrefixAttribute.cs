// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// Annotates a controller with a route prefix that applies to all actions within the controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoutePrefixAttribute" /> class.
        /// </summary>
        /// <param name="prefix">The route prefix for the controller.</param>
        public RoutePrefixAttribute(string prefix)
        {
            if (prefix == null)
            {
                throw Error.ArgumentNull("prefix");
            }

            Prefix = prefix;
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public string Prefix { get; private set; }
    }
}