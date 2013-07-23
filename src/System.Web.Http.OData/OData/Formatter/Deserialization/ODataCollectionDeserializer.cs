// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData collection payloads.
    /// </summary>
    public class ODataCollectionDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCollectionDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The collection type that this deserializer can read.</param>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataCollectionDeserializer(IEdmCollectionTypeReference edmType, ODataDeserializerProvider deserializerProvider)
            : base(edmType, ODataPayloadKind.Collection, deserializerProvider)
        {
            CollectionType = edmType;
            ElementType = edmType.ElementType();
        }

        /// <summary>
        /// Gets the collection type that this deserializer can read.
        /// </summary>
        public IEdmCollectionTypeReference CollectionType { get; private set; }

        /// <summary>
        /// Gets the element type of the collection type that this deserializer can read.
        /// </summary>
        public IEdmTypeReference ElementType { get; private set; }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEnumerable result = ReadInline(ReadCollection(messageReader), readContext) as IEnumerable;
            if (result != null && readContext.IsUntyped && ElementType.IsComplex())
            {
                EdmComplexObjectCollection complexCollection = new EdmComplexObjectCollection(CollectionType);
                foreach (EdmComplexObject complexObject in result)
                {
                    complexCollection.Add(complexObject);
                }
                return complexCollection;
            }

            return result;
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            ODataCollectionValue collection = item as ODataCollectionValue;

            if (collection == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataCollectionValue).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadCollectionValue(collection, readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="collectionValue"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="collectionValue">The <see cref="ODataCollectionValue"/> to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized collection.</returns>
        public virtual IEnumerable ReadCollectionValue(ODataCollectionValue collectionValue, ODataDeserializerContext readContext)
        {
            if (collectionValue == null)
            {
                throw Error.ArgumentNull("collectionValue");
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(ElementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, ElementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            foreach (object entry in collectionValue.Items)
            {
                if (ElementType.IsPrimitive())
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

            ODataCollectionReader reader = messageReader.CreateODataCollectionReader(CollectionType.ElementType());
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
