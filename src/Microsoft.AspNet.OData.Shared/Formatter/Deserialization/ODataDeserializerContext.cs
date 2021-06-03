//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>.
    /// </summary>
    public partial class ODataDeserializerContext
    {
        private bool? _isDeltaOfT;
        private bool? _isDeletedDeltaOfT;
        private bool? _isUntyped;
        private bool? _isChangedObjectCollection;
        private bool? _isDeltaEntity;
        private bool? _isDeltaDeletedEntity;

        /// <summary>
        /// Gets or sets the type of the top-level object the request needs to be deserialized into.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmTypeReference"/> of the top-level object the request needs to be deserialized into.
        /// </summary>
        public IEdmTypeReference ResourceEdmType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Request that is being deserialized.
        /// </summary>
        internal IWebApiRequestMessage InternalRequest { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable case-insensitive request property binding.
        /// </summary>
        internal bool DisableCaseInsensitiveRequestPropertyBinding { get; set; }

        internal bool IsDeltaOfT
        {
            get
            {
                if (!_isDeltaOfT.HasValue)
                {
                    _isDeltaOfT = ResourceType != null && TypeHelper.IsGenericType(ResourceType) && (ResourceType.GetGenericTypeDefinition() == typeof(Delta<>) || 
                        ResourceType.GetGenericTypeDefinition() == typeof(DeltaDeletedEntityObject<>));
                }

                return _isDeltaOfT.Value;
            }
        }

        internal bool IsDeletedDeltaOfT
        {
            get
            {
                if (!_isDeletedDeltaOfT.HasValue)
                {
                    _isDeletedDeltaOfT = ResourceType != null && TypeHelper.IsGenericType(ResourceType) && (ResourceType.GetGenericTypeDefinition() == typeof(DeltaDeletedEntityObject<>) ||
                        ResourceType.GetGenericTypeDefinition() == typeof(DeltaDeletedEntityObject<>));
                }

                return _isDeletedDeltaOfT.Value;
            }
        }

        internal bool IsDeltaEntity
        {
            get
            {
                if (!_isDeltaEntity.HasValue)
                {
                    _isDeltaEntity = ResourceType != null && (ResourceType == typeof(EdmDeltaEntityObject) || ResourceType == typeof(EdmDeltaDeletedEntityObject));
                }

                return _isDeltaEntity.Value;
            }
        }

        internal bool IsDeltaDeletedEntity
        {
            get
            {
                if (!_isDeltaDeletedEntity.HasValue)
                {
                    _isDeltaDeletedEntity = ResourceType != null && ResourceType == typeof(EdmDeltaDeletedEntityObject);
                }

                return _isDeltaDeletedEntity.Value;
            }
        }

        internal bool IsChangedObjectCollection
        {
            get
            {
                if (!_isChangedObjectCollection.HasValue)
                {
                    _isChangedObjectCollection = ResourceType != null && (ResourceType == typeof(EdmChangedObjectCollection) || (TypeHelper.IsGenericType(ResourceType) && 
                        ResourceType.GetGenericTypeDefinition() == typeof(DeltaSet<>) ));
                }

                return _isChangedObjectCollection.Value;
            }
        }

        internal bool IsUntyped
        {
            get
            {
                if (!_isUntyped.HasValue)
                {
                    _isUntyped = IsChangedObjectCollection ? !TypeHelper.IsGenericType(ResourceType) : (TypeHelper.IsTypeAssignableFrom(typeof(IEdmObject), ResourceType) && !IsDeltaOfT) ||
                        typeof(ODataUntypedActionParameters) == ResourceType;
                }

                return _isUntyped.Value;
            }
        }

        internal IEdmTypeReference GetEdmType(Type type)
        {
            if (ResourceEdmType != null)
            {
                return ResourceEdmType;
            }

            return EdmLibHelpers.GetExpectedPayloadType(type, Path, Model);
        }
    }
}
