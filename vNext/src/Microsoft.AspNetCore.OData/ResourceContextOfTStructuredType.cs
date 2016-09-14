// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// An instance of <see cref="ResourceContext{TStructuredType}"/> gets passed to the self link (<see cref="M:EntitySetConfiguration.HasIdLink"/>, <see cref="M:EntitySetConfiguration.HasEditLink"/>, <see cref="M:EntitySetConfiguration.HasReadLink"/>)
    /// and navigation link (<see cref="M:EntitySetConfiguration.HasNavigationPropertyLink"/>, <see cref="M:EntitySetConfiguration.HasNavigationPropertiesLink"/>) builders and can be used by the link builders to generate links.
    /// </summary>
    /// <typeparam name="TStructuredType">The structural type</typeparam>
    public class ResourceContext<TStructuredType> : ResourceContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceContext{TStructuredType}"/> class.
        /// </summary>
        public ResourceContext()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the resource instance.
        /// </summary>
        public new TStructuredType ResourceInstance
        {
            get
            {
                return (TStructuredType)base.ResourceInstance;
            }
            set
            {
                base.ResourceInstance = value;
            }
        }
    }
}
