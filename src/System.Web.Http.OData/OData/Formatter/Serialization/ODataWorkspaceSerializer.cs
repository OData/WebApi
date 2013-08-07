// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="ODataWorkspace" />'s for generating servicedoc's.
    /// </summary>
    public class ODataWorkspaceSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataWorkspaceSerializer"/>.
        /// </summary>
        public ODataWorkspaceSerializer()
            : base(ODataPayloadKind.ServiceDocument)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }

            ODataWorkspace workspace = graph as ODataWorkspace;
            if (workspace == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, type.Name));
            }

            messageWriter.WriteServiceDocument(workspace);
        }
    }
}
