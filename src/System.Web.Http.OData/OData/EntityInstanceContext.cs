// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        /// Gets the <see cref="IEdmModel"/>.
        /// </summary>
        public IEdmModel EdmModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySet"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntityType"/> of this entity instance.
        /// </summary>
        public IEdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets the value of this entity instance.
        /// </summary>
        public object EntityInstance { get; set; }

        /// <summary>
        /// Gets the <see cref="UrlHelper"/> to be used for generating links while serializing this entity instance.
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> to be used for generating OData paths while serializing this entity instance.
        /// </summary>
        public IODataPathHandler PathHandler { get; set; }

        /// <summary>
        /// Gets whether ActionAvailabilityChecks should be performed or not.
        /// This is used to tell the formatter whether to check availability of an action before including a link to it.
        /// When in a feed we skip this check.
        /// </summary>
        public bool SkipExpensiveAvailabilityChecks { get; set; }
    }
}
