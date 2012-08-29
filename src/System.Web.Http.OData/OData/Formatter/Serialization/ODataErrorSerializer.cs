// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class ODataErrorSerializer : ODataSerializer
    {
        public ODataErrorSerializer()
            : base(ODataPayloadKind.Error)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }

            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            ODataError odataError = graph as ODataError;          
            if (odataError == null)
            {
                throw Error.InvalidOperation(SRResources.ErrorTypeMustBeODataError, graph.GetType().FullName);
            }

            bool includeDebugInformation = odataError.InnerError != null;
            messageWriter.WriteError(odataError, includeDebugInformation);
        }
    }
}
