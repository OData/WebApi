// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An return value for SelectController.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should be find an alternative.
    /// </remarks>
    internal class SelectControllerResult
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
