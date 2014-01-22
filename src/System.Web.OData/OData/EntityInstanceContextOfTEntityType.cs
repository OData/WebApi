// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData
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
        /// Gets or sets the entity instance.
        /// </summary>
        [Obsolete("Entity instance might not be available when the incoming uri has a $select. Use the EdmObject property instead.")]
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
