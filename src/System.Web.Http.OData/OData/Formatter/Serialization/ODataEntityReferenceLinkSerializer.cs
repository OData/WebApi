// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing navigation links. For example, the response to the url
    /// http://localhost/Products(10)/$links/Category gets serialized using this.
    /// </summary>
    internal class ODataEntityReferenceLinkSerializer : ODataSerializer
    {
        public ODataEntityReferenceLinkSerializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        {
        }

        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerWriteContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = graph as Uri });
        }
    }
}
