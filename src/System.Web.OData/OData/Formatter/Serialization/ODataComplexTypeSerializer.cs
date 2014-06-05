// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
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

            IEdmComplexObject complexObject = graph as IEdmComplexObject ?? new TypedEdmComplexObject(graph, complexType, writeContext.Model);

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

            // Try to add the dynamic properties if the complex type is open.
            if (complexType.ComplexDefinition().IsOpen)
            {
                AppendDynamicProperties(graph, complexType, writeContext, propertyCollection);
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

        internal void AppendDynamicProperties(object graph, IEdmComplexTypeReference complexType,
            ODataSerializerContext writeContext, List<ODataProperty> propertyCollection)
        {
            PropertyInfo dynamicPropertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(complexType.ComplexDefinition(),
                writeContext.Model);

            if (dynamicPropertyInfo != null)
            {
                IDictionary<string, object> dynamicPropertyDictionary = dynamicPropertyInfo.GetValue(graph)
                    as IDictionary<string, object>;

                if (dynamicPropertyDictionary != null)
                {
                    // build a HashSet to store the declared property names.
                    // It is used to make sure the dynamic property name is different with the declared property name.
                    HashSet<string> declaredPropertyNameSet = new HashSet<string>(propertyCollection.Select(a => a.Name));

                    foreach (KeyValuePair<string, object> dynamicProperty in dynamicPropertyDictionary)
                    {
                        if (dynamicProperty.Value == null)
                        {
                            continue; // skip the null object
                        }

                        Type valueType = dynamicProperty.Value.GetType();
                        IEdmTypeReference edmTypeReference = writeContext.Model.GetEdmTypeReference(valueType);
                        ODataEdmTypeSerializer propertySerializer = SerializerProvider.GetEdmTypeSerializer(edmTypeReference);
                        if (propertySerializer == null)
                        {
                            throw Error.NotSupported(SRResources.TypeCannotBeSerialized,
                                valueType.FullName, typeof(ODataComplexTypeSerializer).Name);
                        }

                        // try to make sure the dynamic property name is not used as declared property name.
                        if (declaredPropertyNameSet.Contains(dynamicProperty.Key))
                        {
                            throw Error.InvalidOperation(SRResources.DynamicPropertyNameAlreadyUsedAsDeclaredPropertyName,
                                dynamicProperty.Key, complexType.FullName());
                        }

                        propertyCollection.Add(propertySerializer.CreateProperty(
                            dynamicProperty.Value, edmTypeReference, dynamicProperty.Key, writeContext));
                    }
                }
            }
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataComplexValue value, ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

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
