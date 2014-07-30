// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing complex types.
    /// </summary>
    public class ODataComplexTypeSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataComplexTypeSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The serializer provider to use to serialize nested objects.</param>
        public ODataComplexTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Property, serializerProvider)
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
            if (writeContext.RootElementName == null)
            {
                throw Error.Argument("writeContext", SRResources.RootElementNameMissing, typeof(ODataSerializerContext).Name);
            }

            IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
            Contract.Assert(edmType != null);

            ODataProperty property = CreateProperty(graph, edmType, writeContext.RootElementName, writeContext);
            messageWriter.WriteProperty(property);
        }

        /// <inheitdoc />
        public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType,
            ODataSerializerContext writeContext)
        {
            if (expectedType == null)
            {
                throw Error.ArgumentNull("expectedType");
            }

            if (!expectedType.IsComplex())
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, expectedType.FullName()));
            }

            return CreateODataComplexValue(graph, expectedType.AsComplex(), writeContext);
        }

        /// <summary>
        /// Creates an <see cref="ODataComplexValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The value of the <see cref="ODataComplexValue"/> to be created.</param>
        /// <param name="complexType">The EDM complex type of the object.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataComplexValue"/>.</returns>
        public virtual ODataComplexValue CreateODataComplexValue(object graph, IEdmComplexTypeReference complexType,
            ODataSerializerContext writeContext)
        {
            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            if (graph == null || graph is NullEdmComplexObject)
            {
                return null;
            }

            IEdmComplexObject complexObject = graph as IEdmComplexObject ?? new TypedEdmComplexObject(graph, complexType);

            List<ODataProperty> propertyCollection = new List<ODataProperty>();
            foreach (IEdmProperty property in complexType.ComplexDefinition().Properties())
            {
                IEdmTypeReference propertyType = property.Type;
                ODataEdmTypeSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(propertyType);
                if (propertySerializer == null)
                {
                    throw Error.NotSupported(SRResources.TypeCannotBeSerialized, propertyType.FullName(), typeof(ODataMediaTypeFormatter).Name);
                }

                object propertyValue;
                if (complexObject.TryGetPropertyValue(property.Name, out propertyValue))
                {
                    propertyCollection.Add(
                        propertySerializer.CreateProperty(propertyValue, property.Type, property.Name, writeContext));
                }
            }

            string typeName = complexType.FullName();

            ODataComplexValue value = new ODataComplexValue()
            {
                Properties = propertyCollection,
                TypeName = typeName
            };

            AddTypeNameAnnotationAsNeeded(value, writeContext.MetadataLevel);
            return value;
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
