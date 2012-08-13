// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public abstract class ODataDeserializer
    {
        protected ODataDeserializer(ODataPayloadKind payloadKind)
        {
            ODataPayloadKind = payloadKind;
        }

        /// <summary>
        /// The kind of ODataPayload this deserializer expects
        /// </summary>
        public ODataPayloadKind ODataPayloadKind { get; private set; }

        /// <summary>
        /// Read an <see cref="IODataRequestMessage"/> using messageReader
        /// </summary>
        /// <param name="messageReader">The messageReader to use</param>
        /// <param name="readContext">The read context</param>
        /// <returns></returns>
        public virtual object Read(ODataMessageReader messageReader, ODataDeserializerReadContext readContext)
        {
            throw Error.NotSupported(SRResources.DeserializerDoesNotSupportRead, this.GetType().Name);
        }
    }
}
