// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an annotation to add the queryable configuration on an EDM navigation property, including auto expanded.
    /// </summary>
    public class NavigationPropertyQueryableConfigurationAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NavigationPropertyQueryableConfigurationAnnotation"/> class.
        /// </summary>
        /// <param name="configuration">The queryable configuration for the EDM navigation property.</param>
        public NavigationPropertyQueryableConfigurationAnnotation(NavigationPropertyQueryableConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration for the EDM property.
        /// </summary>
        public NavigationPropertyQueryableConfiguration Configuration { get; private set; }
    }
}
