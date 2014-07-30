// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> to serialize <see cref="ODataError"/>s and <see cref="HttpError"/>s.
    /// </summary>
    public class ODataErrorSerializer : ODataSerializer
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

            ODataError oDataError = graph as ODataError;
            if (oDataError == null)
            {
                HttpError httpError = graph as HttpError;
                if (httpError == null)
                {
                    string message = Error.Format(SRResources.ErrorTypeMustBeODataErrorOrHttpError, graph.GetType().FullName);
                    throw new SerializationException(message);
                }
                else
                {
                    oDataError = httpError.CreateODataError();
                }
            }

            bool includeDebugInformation = oDataError.InnerError != null;
            messageWriter.WriteError(oDataError, includeDebugInformation);
        }
    }
}
