// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all <see cref="ODataDeserializer" />s that deserialize into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEdmTypeDeserializer : ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of OData payload that this deserializer reads.</param>
        protected ODataEdmTypeDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            EdmType = edmType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of OData payload this deserializer handles.</param>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/>.</param>
        protected ODataEdmTypeDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)
            : this(edmType, payloadKind)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }

            DeserializerProvider = deserializerProvider;
        }

        /// <summary>
        /// Gets the EDM type that this deserializer reads.
        /// </summary>
        public IEdmTypeReference EdmType { get; private set; }

        /// <summary>
        /// The <see cref="ODataDeserializerProvider"/> to use for deserializing inner items.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <summary>
        /// Deserializes the item into a new object of type corresponding to <see cref="EdmType"/>.
        /// </summary>
        /// <param name="item">The item to deserialize.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/></param>
        /// <returns>The deserialized object.</returns>
        public virtual object ReadInline(object item, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, GetType().Name);
        }
    }
}
