// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// A ODataEntrySerializer is an ODataSerializer that serializes instances of <see cref="IEdmType"/>'s.
    /// </summary>
    internal abstract class ODataEntrySerializer : ODataSerializer
    {
        protected ODataEntrySerializer(IEdmTypeReference edmType, ODataPayloadKind odataPayloadKind)
            : base(odataPayloadKind)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            EdmType = edmType;
        }

        protected ODataEntrySerializer(IEdmTypeReference edmType, ODataPayloadKind odataPayloadKind, ODataSerializerProvider serializerProvider)
            : this(edmType, odataPayloadKind)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }

            SerializerProvider = serializerProvider;
        }

        public IEdmTypeReference EdmType { get; private set; }

        public ODataSerializerProvider SerializerProvider { get; private set; }
    }
}
