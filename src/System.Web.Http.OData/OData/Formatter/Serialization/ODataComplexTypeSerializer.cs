// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing complex types.
    /// </summary>
    internal class ODataComplexTypeSerializer : ODataEntrySerializer
    {
        private readonly IEdmComplexTypeReference _edmComplexType;

        public ODataComplexTypeSerializer(IEdmComplexTypeReference edmComplexType, ODataSerializerProvider serializerProvider)
            : base(edmComplexType, ODataPayloadKind.Property, serializerProvider)
        {
            _edmComplexType = edmComplexType;
        }

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

            ODataProperty property = CreateProperty(graph, writeContext.RootElementName, writeContext);

            messageWriter.WriteProperty(property);
        }

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

            if (graph == null)
            {
                return new ODataProperty() { Name = elementName, Value = null };
            }
            else
            {
                List<ODataProperty> propertyCollection = new List<ODataProperty>();
                foreach (IEdmProperty property in _edmComplexType.ComplexDefinition().Properties())
                {
                    IEdmTypeReference propertyType = property.Type;
                    ODataSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(propertyType);
                    if (propertySerializer == null)
                    {
                        throw Error.NotSupported("Type {0} is not a serializable type", propertyType.FullName());
                    }

                    // TODO 453795: [OData]Cleanup reflection code in the ODataFormatter.
                    object propertyValue = graph.GetType().GetProperty(property.Name).GetValue(graph, index: null);

                    propertyCollection.Add(propertySerializer.CreateProperty(propertyValue, property.Name, writeContext));
                }

                string typeName = _edmComplexType.FullName();

                ODataComplexValue value = new ODataComplexValue()
                {
                    Properties = propertyCollection,
                    TypeName = typeName
                };

                AddTypeNameAnnotationAsNeeded(value, writeContext.MetadataLevel);

                return new ODataProperty()
                {
                    Name = elementName,
                    Value = value
                };
            }
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataComplexValue value, ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(value != null);

            if (ShouldAddTypeNameAnnotation(metadataLevel))
            {
                string typeName;

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
                case ODataMetadataLevel.Default:
                case ODataMetadataLevel.MinimalMetadata:
                    return false;
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
