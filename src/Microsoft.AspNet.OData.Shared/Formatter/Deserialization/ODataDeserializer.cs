//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// An <see cref="ODataDeserializer"/> is used to read an ODataMessage into a CLR object.
    /// </summary>
    /// <remarks>
    /// Each supported CLR type has a corresponding <see cref="ODataDeserializer" />. A CLR type is supported if it is one of
    /// the special types or if it has a backing EDM type. Some of the special types are Uri which maps to ODataReferenceLink payload, 
    /// Uri[] which maps to ODataReferenceLinks payload, ODataWorkspace which maps to ODataServiceDocument payload.
    /// </remarks>
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
        /// Reads an <see cref="IODataRequestMessage"/> using messageReader.
        /// </summary>
        /// <param name="messageReader">The messageReader to use.</param>
        /// <param name="type">The type of the object to read into.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The deserialized object.</returns>
        public virtual object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DeserializerDoesNotSupportRead, GetType().Name);
        }

        /// <summary>
        /// Reads an <see cref="IODataRequestMessage"/> using messageReader.
        /// </summary>
        /// <param name="messageReader">The messageReader to use.</param>
        /// <param name="type">The type of the object to read into.</param>
        /// <param name="readContext">The read context.</param>
        /// <returns>The deserialized object.</returns>
        public virtual async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            // This should always be overwritten in the derived class. It is virtual in 7.x only for backward compatability.
            return await Task.Run(() => Read(messageReader, type, readContext));
        }
    }
}
