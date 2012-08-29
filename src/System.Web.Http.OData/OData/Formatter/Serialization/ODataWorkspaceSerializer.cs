// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing <see cref="ODataWorkspace" />'s for generating servicedoc's.
    /// </summary>
    internal class ODataWorkspaceSerializer : ODataSerializer
    {
        public ODataWorkspaceSerializer()
            : base(ODataPayloadKind.ServiceDocument)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            messageWriter.WriteServiceDocument(graph as ODataWorkspace);
        }
    }
}
