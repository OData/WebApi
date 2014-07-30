// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all <see cref="ODataDeserializer" />s that deserialize into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEdmTypeDeserializer : ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this deserializer reads.</param>
        protected ODataEdmTypeDeserializer(ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeDeserializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload this deserializer handles.</param>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/>.</param>
        protected ODataEdmTypeDeserializer(ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)
            : this(payloadKind)
        {
            if (deserializerProvider == null)
            {
                throw Error.ArgumentNull("deserializerProvider");
            }

            DeserializerProvider = deserializerProvider;
        }

        /// <summary>
        /// The <see cref="ODataDeserializerProvider"/> to use for deserializing inner items.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <summary>
        /// Deserializes the item into a new object of type corresponding to <paramref name="edmType"/>.
        /// </summary>
        /// <param name="item">The item to deserialize.</param>
        /// <param name="edmType">The EDM type of the object to read into.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/>.</param>
        /// <returns>The deserialized object.</returns>
        public virtual object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, GetType().Name);
        }
    }
}
