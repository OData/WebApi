// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an OData deserializer.
    /// </summary>
    public abstract class ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataDeserializer"/> class.
        /// </summary>
        /// <param name="payloadKind">The kind of payload this deserializer handles.</param>
        protected ODataDeserializer(ODataPayloadKind payloadKind)
        {
            ODataPayloadKind = payloadKind;
        }

        /// <summary>
        /// The kind of ODataPayload this deserializer handles.
        /// </summary>
        public ODataPayloadKind ODataPayloadKind { get; private set; }

        /// <summary>
        /// Read an <see cref="IODataRequestMessage"/> using messageReader.
        /// </summary>
        /// <param name="messageReader">The messageReader to use.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The deserialized object.</returns>
        public virtual object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DeserializerDoesNotSupportRead, GetType().Name);
        }
    }
}
