// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataSerializer is used to write a CLR object to an ODataMessage.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="ODataSerializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, ODataWorkspace which maps to ODataServiceDocument payload.
    /// </remarks>
    public abstract class ODataSerializer
    {
        /// <summary>
        /// Constructs an ODataSerializer that can generate OData payload of the specified kind.
        /// </summary>
        /// <param name="payloadKind">The kind of OData payload that this serializer generates.</param>
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
        /// <param name="type">The type of the object to be written.</param>
        /// <param name="messageWriter">The <see cref="ODataMessageWriter"/> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            throw Error.NotSupported(SRResources.WriteObjectNotSupported, GetType().Name);
        }
    }
}
