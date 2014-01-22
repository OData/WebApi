// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="ODataServiceDocument" />'s for generating servicedoc's.
    /// </summary>
    public class ODataServiceDocumentSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataServiceDocumentSerializer"/>.
        /// </summary>
        public ODataServiceDocumentSerializer()
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

            ODataServiceDocument serviceDocument = graph as ODataServiceDocument;
            if (serviceDocument == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, type.Name));
            }

            messageWriter.WriteServiceDocument(serviceDocument);
        }
    }
}
