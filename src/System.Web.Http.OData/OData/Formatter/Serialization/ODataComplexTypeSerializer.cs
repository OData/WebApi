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
                    ODataEntrySerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(propertyType);
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
                // For complex types, the default behavior matches the requirements for minimal metadata mode, so no
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
