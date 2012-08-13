// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public abstract class ODataDeserializerProvider
    {
        private ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer> _deserializerCache;

        protected ODataDeserializerProvider(IEdmModel edmModel)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull("edmModel");
            }

            _deserializerCache = new ConcurrentDictionary<IEdmTypeReference, ODataEntryDeserializer>(new EdmTypeReferenceEqualityComparer());
            EdmModel = edmModel;
        }

        public IEdmModel EdmModel { get; private set; }

        public ODataEntryDeserializer GetODataDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            return _deserializerCache.GetOrAdd(edmType, CreateDeserializer);
        }

        protected abstract ODataEntryDeserializer CreateDeserializer(IEdmTypeReference type);

        public abstract ODataDeserializer GetODataDeserializer(Type type);
    }
}
