// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Used to configure the binding for a navigation property for a navigation source.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see
    /// cref="ODataModelBuilder"/>.
    /// </summary>
    public class NavigationPropertyBindingConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyBindingConfiguration"/> class.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for the binding.</param>
        /// <param name="navigationSource">The target navigation source of the binding.</param>
        public NavigationPropertyBindingConfiguration(NavigationPropertyConfiguration navigationProperty,
            NavigationSourceConfiguration navigationSource)
            : this(navigationProperty, navigationSource, new object[] { navigationProperty.PropertyInfo })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyBindingConfiguration"/> class.
        /// </summary>
        /// <param name="navigationProperty">The navigation property for the binding.</param>
        /// <param name="navigationSource">The target navigation source of the binding.</param>
        /// <param name="path">The path of current binding.</param>
        public NavigationPropertyBindingConfiguration(NavigationPropertyConfiguration navigationProperty,
            NavigationSourceConfiguration navigationSource, IList<object> path)
        {
            if (navigationProperty == null)
            {
                throw new ArgumentNullException("navigationProperty");
            }

            if (navigationSource == null)
            {
                throw new ArgumentNullException("navigationSource");
            }

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            NavigationProperty = navigationProperty;
            TargetNavigationSource = navigationSource;
            Path = path;
        }

        /// <summary>
        /// Gets the navigation property of the binding.
        /// </summary>
        public NavigationPropertyConfiguration NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the target navigation source of the binding.
        /// </summary>
        public NavigationSourceConfiguration TargetNavigationSource { get; private set; }

        /// <summary>
        /// Gets the path of current binding.
        /// </summary>
        public IList<object> Path { get; private set; }

        /// <summary>
        /// Gets the path of current binding, like "A.B/C/D.E".
        /// </summary>
        public string BindingPath
        {
            get { return Path.ConvertBindingPath(); }
        }
    }
}
