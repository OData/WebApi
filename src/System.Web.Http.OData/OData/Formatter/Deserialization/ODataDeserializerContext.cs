// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>
    /// from the <see cref="ODataMediaTypeFormatter"/>.
    /// </summary>
    public class ODataDeserializerContext
    {
        /// <summary>
        /// Gets or sets whether the <see cref="ODataMediaTypeFormatter"/> is reading a 
        /// PATCH request.
        /// </summary>
        public bool IsPatchMode { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="Delta{TBaseEntityType}"/> being patched.
        /// </summary>
        public Type PatchEntityType { get; set; }

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
        public HttpRequestMessage Request { get; set; }
    }
}
