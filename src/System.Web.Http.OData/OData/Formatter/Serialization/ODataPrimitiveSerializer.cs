// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing <see cref="IEdmPrimitiveType" />'s.
    /// </summary>
    internal class ODataPrimitiveSerializer : ODataEntrySerializer
    {
        public ODataPrimitiveSerializer(IEdmPrimitiveTypeReference edmPrimitiveType)
            : base(edmPrimitiveType, ODataPayloadKind.Property)
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

            messageWriter.WriteProperty(
                CreateProperty(graph, writeContext.ResponseContext.ServiceOperationName, writeContext));
        }

        public override ODataProperty CreateProperty(object graph, string elementName, ODataSerializerWriteContext writeContext)
        {
            if (String.IsNullOrWhiteSpace(elementName))
            {
                throw Error.ArgumentNullOrEmpty("elementName");
            }

            // TODO: Bug 467598: validate the type of the object being passed in here with the underlying primitive type. 
            return new ODataProperty() { Value = graph, Name = elementName };
        }
    }
}
