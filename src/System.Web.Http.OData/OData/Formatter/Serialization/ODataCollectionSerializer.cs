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
    internal class ODataCollectionSerializer : ODataEntrySerializer
    {
        private readonly IEdmCollectionTypeReference _edmCollectionType;
        private readonly IEdmTypeReference _edmItemType;

        public ODataCollectionSerializer(IEdmCollectionTypeReference edmCollectionType, ODataSerializerProvider serializerProvider)
            : base(edmCollectionType, ODataPayloadKind.Collection, serializerProvider)
        {
            Contract.Assert(edmCollectionType != null);
            _edmCollectionType = edmCollectionType;
            IEdmTypeReference itemType = edmCollectionType.ElementType();
            Contract.Assert(itemType != null);
            _edmItemType = itemType;
        }

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

            ODataCollectionWriter writer = messageWriter.CreateODataCollectionWriter(_edmItemType);
            writer.WriteStart(
                new ODataCollectionStart
                {
                    Name = writeContext.RootElementName
                });

            ODataProperty property = CreateProperty(graph, writeContext.RootElementName, writeContext);

            Contract.Assert(property != null);

            ODataCollectionValue collectionValue = property.Value as ODataCollectionValue;

            Contract.Assert(collectionValue != null);

            foreach (object item in collectionValue.Items)
            {
                writer.WriteItem(item);
            }

            writer.WriteEnd();
            writer.Flush();
        }

        /// <inheritdoc/>
        public override ODataProperty CreateProperty(object graph, string elementName, ODataSerializerContext writeContext)
        {
            if (String.IsNullOrWhiteSpace(elementName))
            {
                throw Error.ArgumentNullOrEmpty("elementName");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            ArrayList valueCollection = new ArrayList();

            IEdmTypeReference itemType = _edmCollectionType.ElementType();
            ODataSerializer itemSerializer = SerializerProvider.GetEdmTypeSerializer(itemType);
            if (itemSerializer == null)
            {
                throw Error.NotSupported(SRResources.TypeCannotBeSerialized, itemType.FullName(), typeof(ODataMediaTypeFormatter).Name);
            }

            IEnumerable enumerable = graph as IEnumerable;

            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    valueCollection.Add(itemSerializer.CreateProperty(item, ODataFormatterConstants.Element, writeContext).Value);
                }
            }

            string typeName = _edmCollectionType.FullName();

            // ODataCollectionValue is only a V3 property, arrays inside Complex Types or Entity types are only supported in V3
            // if a V1 or V2 Client requests a type that has a collection within it ODataLib will throw.
            ODataCollectionValue value = new ODataCollectionValue
            {
                Items = valueCollection,
                TypeName = typeName
            };

            // Required to support JSON light full metadata mode.
            value.SetAnnotation<SerializationTypeNameAnnotation>(
                new SerializationTypeNameAnnotation { TypeName = typeName });

            return new ODataProperty()
            {
                Name = elementName,
                Value = value
            };
        }
    }
}
