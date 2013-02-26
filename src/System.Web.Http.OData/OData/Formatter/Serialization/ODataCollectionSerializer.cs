// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing collection of Entities or Complex types or primitives.
    /// </summary>
    public class ODataCollectionSerializer : ODataEntrySerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCollectionSerializer"/> class.
        /// </summary>
        /// <param name="edmType">The edm collection type this serializer instance can serialize.</param>
        /// <param name="serializerProvider">The serializer provider to use to serialize nested objects.</param>
        public ODataCollectionSerializer(IEdmCollectionTypeReference edmType, ODataSerializerProvider serializerProvider)
            : base(edmType, ODataPayloadKind.Collection, serializerProvider)
        {
            IEdmTypeReference itemType = edmType.ElementType();
            if (itemType == null)
            {
                throw Error.Argument("edmType", SRResources.ItemTypeOfCollectionNull, edmType.FullName());
            }

            CollectionType = edmType;
            ElementType = itemType;
        }

        /// <summary>
        /// Gets the EDM type of the elements of the collection this serializer handles.
        /// </summary>
        public IEdmTypeReference ElementType { get; private set; }

        /// <summary>
        /// Gets the EDM collection type this serializer handles.
        /// </summary>
        public IEdmCollectionTypeReference CollectionType { get; private set; }

        /// <inheritdoc/>
        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            ODataCollectionWriter writer = messageWriter.CreateODataCollectionWriter(ElementType);
            writer.WriteStart(
                new ODataCollectionStart
                {
                    Name = writeContext.RootElementName
                });

            ODataValue value = CreateODataValue(graph, writeContext);
            if (value != null)
            {
                ODataCollectionValue collectionValue = value as ODataCollectionValue;
                Contract.Assert(collectionValue != null);

                foreach (object item in collectionValue.Items)
                {
                    writer.WriteItem(item);
                }
            }

            writer.WriteEnd();
            writer.Flush();
        }

        /// <inheritdoc/>
        public sealed override ODataValue CreateODataValue(object graph, ODataSerializerContext writeContext)
        {
            return CreateODataCollectionValue(graph, writeContext);
        }

        /// <summary>
        /// Creates an <see cref="ODataCollectionValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The value of the collection to be created.</param>
        /// <param name="writeContext">The serializer context to be used while creating the collection.</param>
        /// <returns>The created <see cref="ODataCollectionValue"/>.</returns>
        public virtual ODataCollectionValue CreateODataCollectionValue(object graph, ODataSerializerContext writeContext)
        {
            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            ArrayList valueCollection = new ArrayList();

            IEdmTypeReference itemType = CollectionType.ElementType();
            ODataEntrySerializer itemSerializer = SerializerProvider.GetEdmTypeSerializer(itemType);
            if (itemSerializer == null)
            {
                throw Error.NotSupported(SRResources.TypeCannotBeSerialized, itemType.FullName(), typeof(ODataMediaTypeFormatter).Name);
            }

            IEnumerable enumerable = graph as IEnumerable;

            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    // ODataCollectionWriter expects the individual elements in the collection to be the underlying values
                    // and not ODataValues.
                    valueCollection.Add(itemSerializer.CreateODataValue(item, writeContext).GetInnerValue());
                }
            }

            // Ideally, we'd like to do this:
            // string typeName = _edmCollectionType.FullName();
            // But ODataLib currently doesn't support .FullName() for collections. As a workaround, we construct the
            // collection type name the hard way.
            string typeName = "Collection(" + ElementType.FullName() + ")";

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
    }
}
