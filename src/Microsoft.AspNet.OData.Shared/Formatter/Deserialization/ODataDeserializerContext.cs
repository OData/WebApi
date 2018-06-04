﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        private bool? _isUntyped;

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

        internal bool IsDeltaOfT
        {
            get
            {
                if (!_isDeltaOfT.HasValue)
                {
                    _isDeltaOfT = ResourceType != null && TypeHelper.IsGenericType(ResourceType) && ResourceType.GetGenericTypeDefinition() == typeof(Delta<>);
                }

                return _isDeltaOfT.Value;
            }
        }

        internal bool IsUntyped
        {
            get
            {
                if (!_isUntyped.HasValue)
                {
                    _isUntyped = TypeHelper.IsTypeAssignableFrom(typeof(IEdmObject), ResourceType) ||
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
