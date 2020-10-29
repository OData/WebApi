// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> to serialize <see cref="ODataError"/>s.
    /// </summary>
    public partial class ODataErrorSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the class <see cref="ODataSerializer"/>.
        /// </summary>
        public ODataErrorSerializer()
            : base(ODataPayloadKind.Error)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            ODataError oDataError = GetError(graph);
            bool includeDebugInformation = oDataError.InnerError != null;
            messageWriter.WriteError(oDataError, includeDebugInformation);
        }

        /// <inheritdoc/>
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            ODataError oDataError = GetError(graph);
            bool includeDebugInformation = oDataError.InnerError != null;
            await messageWriter.WriteErrorAsync(oDataError, includeDebugInformation);
        }

        private static ODataError GetError(object graph)
        {
            ODataError oDataError = graph as ODataError;
            if (oDataError == null)
            {
                if (!IsHttpError(graph))
                {
                    string message = Error.Format(SRResources.ErrorTypeMustBeODataErrorOrHttpError, graph.GetType().FullName);
                    throw new SerializationException(message);
                }
                else
                {
                    oDataError = CreateODataError(graph);
                }
            }

            return oDataError;
        }
    }
}
