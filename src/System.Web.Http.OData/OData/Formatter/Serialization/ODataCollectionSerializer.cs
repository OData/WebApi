// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing collection of Entities or Complex types or primitives.
    /// </summary>
    public class ODataCollectionSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCollectionSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use to serialize nested objects.</param>
        public ODataCollectionSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Collection, serializerProvider)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter,
            ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            IEdmTypeReference collectionType = writeContext.GetEdmType(graph, type);
            Contract.Assert(collectionType != null);

            IEdmTypeReference elementType = GetElementType(collectionType);
            ODataCollectionWriter writer = messageWriter.CreateODataCollectionWriter(elementType);
            WriteCollection(writer, graph, collectionType.AsCollection(), writeContext);
        }

        /// <inheritdoc/>
        public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType,
            ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                throw new SerializationException(Error.Format(SRResources.NullCollectionsCannotBeSerialized));
            }

            IEnumerable enumerable = graph as IEnumerable;
            if (enumerable == null)
            {
                throw Error.Argument("graph", SRResources.ArgumentMustBeOfType, typeof(IEnumerable).Name);
            }
            if (expectedType == null)
            {
                throw Error.ArgumentNull("expectedType");
            }

            IEdmTypeReference elementType = GetElementType(expectedType);

            return CreateODataCollectionValue(enumerable, elementType, writeContext);
        }

        /// <summary>
        /// Writes the given <paramref name="graph"/> using the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="ODataCollectionWriter"/> to use.</param>
        /// <param name="graph">The collection to write.</param>
        /// <param name="collectionType">The EDM type of the collection.</param>
        /// <param name="writeContext">The serializer context.</param>
        public void WriteCollection(ODataCollectionWriter writer, object graph, IEdmTypeReference collectionType,
            ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            ODataCollectionValue collectionValue = CreateODataValue(graph, collectionType, writeContext) as ODataCollectionValue;

            writer.WriteStart(new ODataCollectionStart { Name = writeContext.RootElementName });

            if (collectionValue != null)
            {
                foreach (object item in collectionValue.Items)
                {
                    writer.WriteItem(item);
                }
            }

            writer.WriteEnd();
        }

        /// <summary>
        /// Creates an <see cref="ODataCollectionValue"/> for the enumerable represented by <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">The value of the collection to be created.</param>
        /// <param name="elementType">The element EDM type of the collection.</param>
        /// <param name="writeContext">The serializer context to be used while creating the collection.</param>
        /// <returns>The created <see cref="ODataCollectionValue"/>.</returns>
        public virtual ODataCollectionValue CreateODataCollectionValue(IEnumerable enumerable, IEdmTypeReference elementType,
            ODataSerializerContext writeContext)
        {
            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            ArrayList valueCollection = new ArrayList();

            if (enumerable != null)
            {
                ODataEdmTypeSerializer itemSerializer = null;
                foreach (object item in enumerable)
                {
                    itemSerializer = itemSerializer ?? SerializerProvider.GetEdmTypeSerializer(elementType);
                    if (itemSerializer == null)
                    {
                        throw new SerializationException(
                            Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
                    }

                    // ODataCollectionWriter expects the individual elements in the collection to be the underlying values and not ODataValues.
                    valueCollection.Add(itemSerializer.CreateODataValue(item, elementType, writeContext).GetInnerValue());
                }
            }

            // Ideally, we'd like to do this:
            // string typeName = _edmCollectionType.FullName();
            // But ODataLib currently doesn't support .FullName() for collections. As a workaround, we construct the
            // collection type name the hard way.
            string typeName = "Collection(" + elementType.FullName() + ")";

            // ODataCollectionValue is only a V3 property, arrays inside Complex Types or Entity types are only supported in V3
            // if a V1 or V2 Client requests a type that has a collection within it ODataLib will throw.
            ODataCollectionValue value = new ODataCollectionValue
            {
                Items = valueCollection,
                TypeName = typeName
            };

            AddTypeNameAnnotationAsNeeded(value, writeContext.MetadataLevel);
            return value;
        }

        /// <summary>
        /// Adds the type name annotations required for proper json light serialization.
        /// </summary>
        /// <param name="value">The collection value for which the annotations have to be added.</param>
        /// <param name="metadataLevel">The OData metadata level of the response.</param>
        protected internal static void AddTypeNameAnnotationAsNeeded(ODataCollectionValue value, ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note that this annotation should not be used for Atom or JSON verbose formats, as it will interfere with
            // the correct default behavior for those formats.

            Contract.Assert(value != null);

            // Only add an annotation if we want to override ODataLib's default type name serialization behavior.
            if (ShouldAddTypeNameAnnotation(metadataLevel))
            {
                string typeName;

                // Provide the type name to serialize (or null to force it not to serialize).
                if (ShouldSuppressTypeNameSerialization(metadataLevel))
                {
                    typeName = null;
                }
                else
                {
                    typeName = value.TypeName;
                }

                value.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
                {
                    TypeName = typeName
                });
            }
        }

        internal static bool ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel)
        {
            switch (metadataLevel)
            {
                // Don't interfere with the correct default behavior in non-JSON light formats.
                case ODataMetadataLevel.Default:
                // For collections, the default behavior matches the requirements for minimal metadata mode, so no
                // annotation is necessary.
                case ODataMetadataLevel.MinimalMetadata:
                    return false;
                // In other cases, this class must control the type name serialization behavior.
                case ODataMetadataLevel.FullMetadata:
                case ODataMetadataLevel.NoMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return true;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(metadataLevel != ODataMetadataLevel.Default);
            Contract.Assert(metadataLevel != ODataMetadataLevel.MinimalMetadata);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return false;
            }
        }

        private static IEdmTypeReference GetElementType(IEdmTypeReference feedType)
        {
            if (feedType.IsCollection())
            {
                return feedType.AsCollection().ElementType();
            }

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataFeedSerializer).Name, feedType.FullName());
            throw new SerializationException(message);
        }
    }
}
