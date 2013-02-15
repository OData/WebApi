// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> that serializes instances of objects backed by an <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEntrySerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntrySerializer"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of odata payload that this serializer generates.</param>
        protected ODataEntrySerializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            EdmType = edmType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntrySerializer"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of odata payload that this serializer generates.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write inner objects.</param>
        protected ODataEntrySerializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind, ODataSerializerProvider serializerProvider)
            : this(edmType, payloadKind)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }

            SerializerProvider = serializerProvider;
        }

        /// <summary>
        /// Gets the EDM type this serializer can write.
        /// </summary>
        public IEdmTypeReference EdmType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that can be used to write inner objects.
        /// </summary>
        public ODataSerializerProvider SerializerProvider { get; private set; }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteObjectInline(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectInlineNotSupported, GetType().Name);
        }

        /// <summary>
        /// Creates an <see cref="ODataProperty"/> that gets written as a part of an <see cref="ODataItem"/> with the given propertyName 
        /// and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="elementName">The name of the property to create.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        /// <returns>The <see cref="ODataProperty"/> created.</returns>
        public virtual ODataProperty CreateProperty(object graph, string elementName, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.CreatePropertyNotSupported, GetType().Name);
        }
    }
}
