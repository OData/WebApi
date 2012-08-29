// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataRawValueDeserializer : ODataEntryDeserializer
    {
        public ODataRawValueDeserializer(IEdmPrimitiveTypeReference edmPrimitiveType)
            : base(edmPrimitiveType, ODataPayloadKind.Value)
        {
            PrimitiveTypeReference = edmPrimitiveType;
        }

        public IEdmPrimitiveTypeReference PrimitiveTypeReference { get; private set; }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            object value = messageReader.ReadValue(PrimitiveTypeReference);

            // TODO: Bug 467612: do value conversions here.
            return value;
        }
    }
}
