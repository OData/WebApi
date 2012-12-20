// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
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
        private const int MaxReferenceDepth = 200;
        private int _currentReferenceDepth = 0;

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
        /// Gets or set the EdmModel associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Increments the current reference depth.
        /// </summary>
        /// <returns><c>false</c> if the current reference depth is greater than the maximum allowed and <c>false</c> otherwise.</returns>
        public bool IncrementCurrentReferenceDepth()
        {
            if (++_currentReferenceDepth > MaxReferenceDepth)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decrements the current reference depth.
        /// </summary>
        public void DecrementCurrentReferenceDepth()
        {
            _currentReferenceDepth--;
            Contract.Assert(_currentReferenceDepth >= 0);
        }
    }
}
