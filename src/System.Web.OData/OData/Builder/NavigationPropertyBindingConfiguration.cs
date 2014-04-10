// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder
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
            INavigationSourceConfiguration navigationSource)
        {
            NavigationProperty = navigationProperty;
            TargetNavigationSource = navigationSource;
        }

        /// <summary>
        /// Gets the navigation property of the binding.
        /// </summary>
        public NavigationPropertyConfiguration NavigationProperty { get; private set; }

        /// <summary>
        /// Gets the target navigation source of the binding.
        /// </summary>
        public INavigationSourceConfiguration TargetNavigationSource { get; private set; }
    }
}
