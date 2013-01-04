// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public abstract class ODataDeserializerProvider
    {
        private readonly ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer> _deserializerCache =
            new ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer>(new EdmTypeReferenceEqualityComparer());

        public ODataEntryDeserializer GetODataDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            return _deserializerCache.GetOrAdd(edmType, CreateDeserializer);
        }

        protected abstract ODataEntryDeserializer CreateDeserializer(IEdmTypeReference type);

        public abstract ODataDeserializer GetODataDeserializer(IEdmModel model, Type type);
    }
}
