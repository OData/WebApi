// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>
    /// from the <see cref="ODataMediaTypeFormatter"/>.
    /// </summary>
    public class ODataDeserializerContext
    {
        // private bool? _isDeltaOfT;
        // private bool? _isUntyped;

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
        public HttpRequest Request { get; set; }

        /// <summary>Gets or sets the request context.</summary>
        public HttpContext RequestContext { get; set; }

        internal bool IsDeltaOfT
        {
            get
            {
                //if (!_isDeltaOfT.HasValue)
                //{
                //    _isDeltaOfT = ResourceType != null && ResourceType.GetTypeInfo().IsGenericType && ResourceType.GetGenericTypeDefinition() == typeof(Delta<>);
                //}

                //return _isDeltaOfT.Value;
                throw new NotImplementedException("IsDeltaOfT");
            }
        }

        internal bool IsUntyped
        {
            get
            {
                //if (!_isUntyped.HasValue)
                //{
                //    _isUntyped = typeof(IEdmObject).IsAssignableFrom(ResourceType) ||
                //        typeof(ODataUntypedActionParameters) == ResourceType;
                //}

                //return _isUntyped.Value;
                throw new NotImplementedException("IsUntyped");
            }
        }

        internal IEdmTypeReference GetEdmType(Type type)
        {
            //if (ResourceEdmType != null)
            //{
            //    return ResourceEdmType;
            //}

            //return ODataMediaTypeFormatter.GetExpectedPayloadType(type, Path, Model);
            throw new NotImplementedException("GetEdmType");
        }
    }
}
