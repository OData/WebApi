// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class ODataMetadataSerializer : ODataSerializer
    {
        public ODataMetadataSerializer()
            : base(ODataPayloadKind.ServiceDocument)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            // NOTE: ODataMessageWriter doesn't have a way to set the IEdmModel. So, there is an underlying assumption here that
            // the model received by this method and the model passed(from configuration) while building ODataMessageWriter is the same (clr object).
            messageWriter.WriteMetadataDocument();
        }
    }
}
