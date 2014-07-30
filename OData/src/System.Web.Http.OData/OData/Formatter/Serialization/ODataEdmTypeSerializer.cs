// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> that serializes instances of objects backed by an <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEdmTypeSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEdmTypeSerializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write inner objects.</param>
        protected ODataEdmTypeSerializer(ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)
            : this(payloadKind)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }

            SerializerProvider = serializerProvider;
        }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that can be used to write inner objects.
        /// </summary>
        public ODataSerializerProvider SerializerProvider { get; private set; }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectInlineNotSupported, GetType().Name);
        }

        /// <summary>
        /// Creates an <see cref="ODataValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The value of the <see cref="ODataValue"/> to be created.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        /// <returns>The <see cref="ODataValue"/> created.</returns>
        public virtual ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.CreateODataValueNotSupported, GetType().Name);
        }

        internal ODataProperty CreateProperty(object graph, IEdmTypeReference expectedType, string elementName,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(elementName != null);
            return new ODataProperty
            {
                Name = elementName,
                Value = CreateODataValue(graph, expectedType, writeContext)
            };
        }
    }
}
