// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An instance of <see cref="EntityInstanceContext{TEntityType}"/> gets passed to the self link (<see cref="M:EntitySetConfiguration.HasIdLink"/>, <see cref="M:EntitySetConfiguration.HasEditLink"/>, <see cref="M:EntitySetConfiguration.HasReadLink"/>)
    /// and navigation link (<see cref="M:EntitySetConfiguration.HasNavigationPropertyLink"/>, <see cref="M:EntitySetConfiguration.HasNavigationPropertiesLink"/>) builders and can be used by the link builders to generate links.
    /// </summary>
    /// <typeparam name="TEntityType">The entity type</typeparam>
    public class EntityInstanceContext<TEntityType> : EntityInstanceContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceContext{TEntityType}"/> class.
        /// </summary>
        public EntityInstanceContext()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceContext{TEntityType}"/> class.
        /// </summary>
        /// <param name="serializerContext">The backing <see cref="ODataSerializerContext"/>.</param>
        /// <param name="entityType">The EDM entity type of this instance context.</param>
        /// <param name="entityInstance">The CLR instance of this instance context.</param>
        public EntityInstanceContext(ODataSerializerContext serializerContext, IEdmEntityType entityType, TEntityType entityInstance)
            : base(serializerContext, entityType, entityInstance)
        {
        }

        /// <summary>
        /// Gets or sets the entity instance.
        /// </summary>
        public new TEntityType EntityInstance
        {
            get
            {
                return (TEntityType)base.EntityInstance;
            }
            set
            {
                base.EntityInstance = value;
            }
        }
    }
}
