//-----------------------------------------------------------------------------
// <copyright file="ODataRoutePrefixAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Represents an attribute that can be placed on an OData controller to specify
    /// the prefix that will be used for all actions of that controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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
