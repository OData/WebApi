// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataSerializer is used to write CLR objects to an ODataMessage. Each supported CLR type has a corresponding <see cref="ODataSerializer" />. A CLR type is 
    /// supported if it is one of the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, ODataWorkspace which maps to ODataServiceDocument payload.
    /// </summary>
    public abstract class ODataSerializer
    {
        /// <summary>
        /// Constructs an ODataSerializer that can generate OData payload of the specified kind.
        /// </summary>
        /// <param name="payloadKind">The kind of odata payload that this serializer generates</param>
        protected ODataSerializer(ODataPayloadKind payloadKind)
        {
            ODataPayloadKindHelper.Validate(payloadKind, "payloadKind");

            ODataPayloadKind = payloadKind;
        }

        /// <summary>
        /// Gets the <see cref="ODataPayloadKind"/> that this serializer generates.
        /// </summary>
        public ODataPayloadKind ODataPayloadKind { get; private set; }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a whole using the given messageWriter and writeContext.
        /// </summary>
        /// <param name="graph">The object to be written</param>
        /// <param name="messageWriter">The <see cref="ODataMessageWriter" /> to be used for writing</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext" /></param>
        public virtual void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectNotSupported, GetType().Name);
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written</param>
        /// <param name="writer">The <see cref="ODataWriter" /> to be used for writing</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext" /></param>
        public virtual void WriteObjectInline(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectInlineNotSupported, GetType().Name);
        }

        /// <summary>
        /// Create a <see cref="ODataProperty" /> that gets written as a part of an <see cref="ODataItem" /> with the given propertyName 
        /// and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written </param>
        /// <param name="elementName">The name of the property to create</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext" /></param>
        /// <returns>The <see cref="ODataProperty" /> created.</returns>
        public virtual ODataProperty CreateProperty(object graph, string elementName, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.CreatePropertyNotSupported, GetType().Name);
        }
    }
}
