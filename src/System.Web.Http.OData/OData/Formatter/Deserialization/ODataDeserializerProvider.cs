// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents a factory that creates an <see cref="ODataDeserializer"/>s.
    /// </summary>
    public abstract class ODataDeserializerProvider
    {
        private readonly ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer> _deserializerCache =
            new ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer>(new EdmTypeReferenceEqualityComparer());

        /// <summary>
        /// Gets the <see cref="ODataDeserializer"/> for the given EDM type.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <returns>An <see cref="ODataEntryDeserializer"/> that can deserialize the given EDM type.</returns>
        public ODataEntryDeserializer GetODataDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            return _deserializerCache.GetOrAdd(edmType, CreateDeserializer);
        }

        /// <summary>
        /// Creates an <see cref="ODataEntryDeserializer"/> for the given EDM type.
        /// </summary>
        /// <param name="type">The EDM type</param>
        /// <returns>An <see cref="ODataEntryDeserializer"/> that can deserialize the given EDM type.</returns>
        protected abstract ODataEntryDeserializer CreateDeserializer(IEdmTypeReference type);

        /// <summary>
        /// Gets an <see cref="ODataDeserializer"/> for the given type.
        /// </summary>
        /// <param name="model">The EDM model.</param>
        /// <param name="type">The CLR type.</param>
        /// <returns>An <see cref="ODataDeserializer"/> that can deserialize the given type.</returns>
        public abstract ODataDeserializer GetODataDeserializer(IEdmModel model, Type type);
    }
}
