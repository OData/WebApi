// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An instance of <see cref="EntityInstanceContext{TEntityType}"/> gets passed to the self link (<see cref="M:EntitySetConfiguration.HasIdLink"/>, <see cref="M:EntitySetConfiguration.HasEditLink"/>, <see cref="M:EntitySetConfiguration.HasReadLink"/>)
    /// and navigation link (<see cref="M:EntitySetConfiguration.HasNavigationPropertyLink"/>, <see cref="M:EntitySetConfiguration.HasNavigationPropertiesLink"/>) builders and can be used by the link builders to generate links.
    /// </summary>
    public class EntityInstanceContext
    {
        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belogs.
        /// </summary>
        public IEdmModel EdmModel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntitySet"/> to which this instance belongs.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntityType"/> of this entity instance.
        /// </summary>
        public IEdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the value of this entity instance.
        /// </summary>
        public object EntityInstance { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="UrlHelper"/> that may be used to generate links while serializing this entity
        /// instance.
        /// </summary>
        public UrlHelper Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ActionAvailabilityChecks should be performed or not.
        /// </summary>
        /// <remarks>
        /// This value is used to tell the formatter whether to check availability of an action before including a link
        /// to it. When in a feed we skip this check.
        /// </remarks>
        public bool SkipExpensiveAvailabilityChecks { get; set; }
    }
}
