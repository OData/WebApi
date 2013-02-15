// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataCollectionDeserializer : ODataEntryDeserializer<ODataCollectionValue>
    {
        private readonly IEdmCollectionTypeReference _edmCollectionType;

        public ODataCollectionDeserializer(IEdmCollectionTypeReference edmCollectionType, ODataDeserializerProvider deserializerProvider)
            : base(edmCollectionType, ODataPayloadKind.Collection, deserializerProvider)
        {
            _edmCollectionType = edmCollectionType;
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

            ODataCollectionValue value = ReadCollection(messageReader);
            return ReadInline(value, readContext);
        }

        public override object ReadInline(ODataCollectionValue collection, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (collection == null)
            {
                return null;
            }

            // Recursion guard to avoid stack overflows
            EnsureStackHelper.EnsureStack();

            return ReadItems(collection, readContext);
        }

        private IEnumerable ReadItems(ODataCollectionValue collection, ODataDeserializerContext readContext)
        {
            IEdmTypeReference elementType = _edmCollectionType.ElementType();
            ODataEntryDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            Contract.Assert(deserializer != null);

            foreach (object entry in collection.Items)
            {
                if (elementType.IsPrimitive())
                {
                    yield return entry;
                }
                else
                {
                    yield return deserializer.ReadInline(entry, readContext);
                }
            }
        }

        private ODataCollectionValue ReadCollection(ODataMessageReader messageReader)
        {
            Contract.Assert(messageReader != null);

            ODataCollectionReader reader = messageReader.CreateODataCollectionReader(_edmCollectionType.ElementType());
            ArrayList items = new ArrayList();
            string typeName = null;

            while (reader.Read())
            {
                if (ODataCollectionReaderState.Value == reader.State)
                {
                    items.Add(reader.Item);
                }
                else if (ODataCollectionReaderState.CollectionStart == reader.State)
                {
                    typeName = reader.Item.ToString();
                }
            }

            return new ODataCollectionValue { Items = items, TypeName = typeName };
        }
    }
}
