// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Formatter.Serialization;
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
        /// Initializes a new instance of the <see cref="EntityInstanceContext"/> class.
        /// </summary>
        public EntityInstanceContext()
        {
            SerializerContext = new ODataSerializerContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceContext"/> class.
        /// </summary>
        /// <param name="serializerContext">The backing <see cref="ODataSerializerContext"/>.</param>
        /// <param name="entityType">The EDM entity type of this instance context.</param>
        /// <param name="entityInstance">The CLR instance of this instance context.</param>
        public EntityInstanceContext(ODataSerializerContext serializerContext, IEdmEntityType entityType, object entityInstance)
        {
            if (serializerContext == null)
            {
                throw Error.ArgumentNull("serializerContext");
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            SerializerContext = serializerContext;
            EntityType = entityType;
            EntityInstance = entityInstance;
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataSerializerContext"/>.
        /// </summary>
        public ODataSerializerContext SerializerContext { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequestMessage Request
        {
            get
            {
                return SerializerContext.Request;
            }
            set
            {
                SerializerContext.Request = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belogs.
        /// </summary>
        public IEdmModel EdmModel
        {
            get
            {
                return SerializerContext.Model;
            }
            set
            {
                SerializerContext.Model = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntitySet"/> to which this instance belongs.
        /// </summary>
        public IEdmEntitySet EntitySet
        {
            get
            {
                return SerializerContext.EntitySet;
            }
            set
            {
                SerializerContext.EntitySet = value;
            }
        }

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
        public UrlHelper Url
        {
            get
            {
                return SerializerContext.Url;
            }
            set
            {
                SerializerContext.Url = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ActionAvailabilityChecks should be performed or not.
        /// </summary>
        /// <remarks>
        /// This value is used to tell the formatter whether to check availability of an action before including a link
        /// to it. When in a feed we skip this check.
        /// </remarks>
        public bool SkipExpensiveAvailabilityChecks
        {
            get
            {
                return SerializerContext.SkipExpensiveAvailabilityChecks;
            }
            set
            {
                SerializerContext.SkipExpensiveAvailabilityChecks = value;
            }
        }
    }
}
