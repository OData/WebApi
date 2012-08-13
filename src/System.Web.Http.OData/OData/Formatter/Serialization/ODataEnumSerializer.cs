// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing instances of <see cref="IEdmEnumType" />
    /// </summary>
    internal class ODataEnumSerializer : ODataEntrySerializer
    {
        public ODataEnumSerializer(IEdmEnumTypeReference edmEnumType, ODataSerializerProvider serializerProvider)
            : base(edmEnumType, ODataPayloadKind.Property, serializerProvider)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerWriteContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            ODataProperty property = CreateProperty(graph, writeContext.ResponseContext.ServiceOperationName, writeContext);
            messageWriter.WriteProperty(property);
        }

        public override ODataProperty CreateProperty(object graph, string elementName, ODataSerializerWriteContext writeContext)
        {
            if (String.IsNullOrWhiteSpace(elementName))
            {
                throw Error.ArgumentNullOrEmpty("elementName");
            }

            string value = null;

            if (graph != null)
            {
                // TODO: Bug 453831: [OData] Figure out how OData serializes enum flags
                value = graph.ToString();
            }

            ODataProperty property = new ODataProperty()
            {
                Name = elementName,
                Value = value
            };

            return property;
        }
    }
}
