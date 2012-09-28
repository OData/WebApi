// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An instance of <see cref="EntityInstanceContext{TEntityType}"/> gets passed to the self link (<see cref="M:IEntitySetConfiguration.HasIdLink"/>, <see cref="M:IEntitySetConfiguration.HasEditLink"/>, <see cref="M:IEntitySetConfiguration.HasReadLink"/>)
    /// and navigation link (<see cref="M:IEntitySetConfiguration.HasNavigationPropertyLink"/>, <see cref="M:IEntitySetConfiguration.HasNavigationPropertiesLink"/>) builders and can be used by the link builders to generate links.
    /// </summary>
    /// <typeparam name="TEntityType">The entity type</typeparam>
    public class EntityInstanceContext<TEntityType> : EntityInstanceContext
    {
        /// <summary>
        /// Parameterless constructor to be used only for unit testing.
        /// </summary>
        public EntityInstanceContext()
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, TEntityType entityInstance)
            : this(model, entitySet, entityType, urlHelper, entityInstance, skipExpensiveActionAvailabilityChecks: false)
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, TEntityType entityInstance, bool skipExpensiveActionAvailabilityChecks)
            : base(model, entitySet, entityType, urlHelper, entityInstance, skipExpensiveAvailabilityChecks: skipExpensiveActionAvailabilityChecks)
        {
        }

        public new TEntityType EntityInstance
        {
            get { return (TEntityType)base.EntityInstance; }
        }
    }
}
