// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    public class EntityInstanceContext<TEntityType> : EntityInstanceContext
    {
        /// <summary>
        /// Parameterless constructor to be used only for unit testing.
        /// </summary>
        public EntityInstanceContext()
        {
        }

        public EntityInstanceContext(IEdmModel model, IEdmEntitySet entitySet, IEdmEntityType entityType, UrlHelper urlHelper, TEntityType entityInstance)
            : base(model, entitySet, entityType, urlHelper, entityInstance)
        {
        }

        public new TEntityType EntityInstance
        {
            get { return (TEntityType)base.EntityInstance; }
        }
    }
}
