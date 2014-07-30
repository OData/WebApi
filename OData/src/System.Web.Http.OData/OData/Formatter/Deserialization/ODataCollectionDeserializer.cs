// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
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
        private static readonly MethodInfo _castMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCollectionDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataCollectionDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Collection, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            if (!edmType.IsCollection())
            {
                throw Error.Argument("type", SRResources.ArgumentMustBeOfType, EdmTypeKind.Collection);
            }

            IEdmCollectionTypeReference collectionType = edmType.AsCollection();
            IEdmTypeReference elementType = collectionType.ElementType();
            ODataCollectionReader reader = messageReader.CreateODataCollectionReader(elementType);
            return ReadInline(ReadCollection(reader), edmType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            if (!edmType.IsCollection())
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, edmType.ToTraceString(), typeof(ODataMediaTypeFormatter)));
            }

            IEdmCollectionTypeReference collectionType = edmType.AsCollection();
            IEdmTypeReference elementType = collectionType.ElementType();

            ODataCollectionValue collection = item as ODataCollectionValue;

            if (collection == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataCollectionValue).Name);
            }
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            IEnumerable result = ReadCollectionValue(collection, elementType, readContext);
            if (result != null)
            {
                if (readContext.IsUntyped && elementType.IsComplex())
                {
                    EdmComplexObjectCollection complexCollection = new EdmComplexObjectCollection(collectionType);
                    foreach (EdmComplexObject complexObject in result)
                    {
                        complexCollection.Add(complexObject);
                    }
                    return complexCollection;
                }
                else
                {
                    Type elementClrType = EdmLibHelpers.GetClrType(elementType, readContext.Model);
                    IEnumerable castedResult = _castMethodInfo.MakeGenericMethod(elementClrType).Invoke(null, new object[] { result }) as IEnumerable;
                    return castedResult;
                }
            }
            return null;
        }

        /// <summary>
        /// Deserializes the given <paramref name="collectionValue"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="collectionValue">The <see cref="ODataCollectionValue"/> to deserialize.</param>
        /// <param name="elementType">The element type of the collection to read.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized collection.</returns>
        public virtual IEnumerable ReadCollectionValue(ODataCollectionValue collectionValue, IEdmTypeReference elementType,
            ODataDeserializerContext readContext)
        {
            if (collectionValue == null)
            {
                throw Error.ArgumentNull("collectionValue");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            foreach (object entry in collectionValue.Items)
            {
                if (elementType.IsPrimitive())
                {
                    yield return entry;
                }
                else
                {
                    yield return deserializer.ReadInline(entry, elementType, readContext);
                }
            }
        }

        internal static ODataCollectionValue ReadCollection(ODataCollectionReader reader)
        {
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
