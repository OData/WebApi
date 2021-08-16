//-----------------------------------------------------------------------------
// <copyright file="SelectControllerResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An return value for SelectController.
    /// </summary>
    public class SelectControllerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectControllerResult"/> class.
        /// </summary>
        /// <param name="controllerName">The controller name selected.</param>
        /// <param name="values">The properties associated with the selected controller.4.</param>
        public SelectControllerResult(string controllerName, IDictionary<string, object> values)
        {
            this.ControllerName = controllerName;
            this.Values = values;
        }

        /// <summary>
        /// Gets the controller name selected.
        /// </summary>
        public string ControllerName { get; private set; }

        /// <summary>
        /// Gets or sets the properties associated with the selected controller.
        /// </summary>
        /// <remarks>By default, Values is null.</remarks>
        public IDictionary<string, object> Values { get; private set; }
    }
}
