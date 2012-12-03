// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataPrimitiveDeserializer : ODataEntryDeserializer<ODataProperty>
    {
        public ODataPrimitiveDeserializer(IEdmPrimitiveTypeReference edmType)
            : base(edmType, ODataPayloadKind.Property)
        {
        }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            // TODO: Feature #664 - Support JSON light (passing the structural property or type reference parameter).
            ODataProperty property = messageReader.ReadProperty();
            return ReadInline(property, readContext);
        }

        public override object ReadInline(ODataProperty property, ODataDeserializerContext readContext)
        {
            if (property != null)
            {
                return property.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
