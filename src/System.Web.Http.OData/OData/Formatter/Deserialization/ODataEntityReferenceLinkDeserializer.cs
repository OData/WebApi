// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataEntityReferenceLinkDeserializer : ODataDeserializer
    {
        public ODataEntityReferenceLinkDeserializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        { 
        }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerReadContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            ODataEntityReferenceLink entityReferenceLink = messageReader.ReadEntityReferenceLink();
            if (entityReferenceLink != null)
            {
                return entityReferenceLink.Url;
            }

            return null;
        }
    }
}
