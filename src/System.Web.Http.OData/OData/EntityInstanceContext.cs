// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    public class EntityInstanceContext
    {
        /// <summary>
        /// Parameterless constructor to be used only for unit testing.
        /// </summary>
        public EntityInstanceContext()
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, object entityInstance)
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
    }
}
