// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
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

        public ODataCollectionSerializer(IEdmCollectionTypeReference edmCollectionType, ODataSerializerProvider serializerProvider)
            : base(edmCollectionType, ODataPayloadKind.Collection, serializerProvider)
        {
            _edmCollectionType = edmCollectionType;
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

            ODataCollectionWriter writer = messageWriter.CreateODataCollectionWriter();
            writer.WriteStart(
                new ODataCollectionStart
                {
                    Name = writeContext.RootElementName
                });

            ODataProperty property = CreateProperty(graph, writeContext.RootElementName, writeContext);
            if (property != null)
            {
                ODataCollectionValue collectionValue = property.Value as ODataCollectionValue;

                foreach (object item in collectionValue.Items)
                {
                    writer.WriteItem(item);
                }

                writer.WriteEnd();
                writer.Flush();
            }
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

            IEnumerable enumerable = graph as IEnumerable;
            if (enumerable == null)
            {
                return new ODataProperty() { Name = elementName, Value = null };
            }
            else
            {
                ArrayList valueCollection = new ArrayList();

                IEdmTypeReference itemType = _edmCollectionType.ElementType();
                ODataSerializer itemSerializer = SerializerProvider.GetEdmTypeSerializer(itemType);
                if (itemSerializer == null)
                {
                    throw Error.NotSupported(SRResources.TypeCannotBeSerialized, itemType.FullName(), typeof(ODataMediaTypeFormatter).Name);
                }

                foreach (object item in enumerable)
                {
                    valueCollection.Add(itemSerializer.CreateProperty(item, ODataFormatterConstants.Element, writeContext).Value);
                }

                // ODataCollectionValue is only a V3 property, arrays inside Complex Types or Entity types are only supported in V3
                // if a V1 or V2 Client requests a type that has a collection within it ODataLIb will throw.
                // Also, note that TypeName is an optional property for ODataCollectionValue
                return new ODataProperty() { Name = elementName, Value = new ODataCollectionValue { Items = valueCollection } };
            }
        }
    }
}
