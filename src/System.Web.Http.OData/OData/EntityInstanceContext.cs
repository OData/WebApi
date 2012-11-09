// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        /// Parameterless constructor to be used only for unit testing.
        /// </summary>
        public EntityInstanceContext()
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, object entityInstance)
            : this(model, entitySet, entityType, urlHelper, entityInstance, skipExpensiveAvailabilityChecks: false)
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, object entityInstance, bool skipExpensiveAvailabilityChecks)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            if (entityInstance == null)
            {
                throw Error.ArgumentNull("entityInstance");
            }

            EdmModel = model;
            EntitySet = entitySet;
            EntityType = entityType;
            EntityInstance = entityInstance;
            UrlHelper = urlHelper;
            SkipExpensiveAvailabilityChecks = skipExpensiveAvailabilityChecks;
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/>.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose.
        /// </summary>
        public IEdmModel EdmModel { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySet"/> this instance belongs to.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntityType"/> of this entity instance.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public IEdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets the value of this entity instance.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public object EntityInstance { get; set; }

        /// <summary>
        /// Gets the <see cref="UrlHelper"/> to be used for generating navigation and self links while
        /// serializing this entity instance.
        /// 
        /// The setter is not intended to be used other than for unit testing purpose. 
        /// </summary>
        public UrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets whether ActionAvailabilityChecks should be performed or not.
        /// This is used to tell the formatter whether to check availability of an action before including a link to it.
        /// When in a feed we skip this check.
        /// 
        /// The setter is not intended to be used other than for unit testing purposes.
        /// </summary>
        public bool SkipExpensiveAvailabilityChecks { get; set; }
    }
}
