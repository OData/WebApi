// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Linq;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Xml.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmPrimitiveType" />'s.
    /// </summary>
    public class ODataPrimitiveSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataPrimitiveSerializer"/>.
        /// </summary>
        public ODataPrimitiveSerializer()
            : base(ODataPayloadKind.Property)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
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

            messageWriter.WriteProperty(CreateProperty(graph, edmType, writeContext.RootElementName, writeContext));
        }

        /// <inheritdoc/>
        public sealed override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            if (!expectedType.IsPrimitive())
            {
                throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataPrimitiveSerializer), expectedType.FullName());
            }

            ODataPrimitiveValue value = CreateODataPrimitiveValue(graph, expectedType.AsPrimitive(), writeContext);
            if (value == null)
            {
                return new ODataNullValue();
            }

            return value;
        }

        /// <summary>
        /// Creates an <see cref="ODataPrimitiveValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The primitive value.</param>
        /// <param name="primitiveType">The EDM primitive type of the value.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The created <see cref="ODataPrimitiveValue"/>.</returns>
        public virtual ODataPrimitiveValue CreateODataPrimitiveValue(object graph, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            ODataMetadataLevel metadataLevel = writeContext != null ? writeContext.MetadataLevel : ODataMetadataLevel.Default;

            // TODO: Bug 467598: validate the type of the object being passed in here with the underlying primitive type. 
            return CreatePrimitive(graph, primitiveType, metadataLevel);
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataPrimitiveValue primitive, IEdmPrimitiveTypeReference primitiveType,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note that this annotation should not be used for Atom or JSON verbose formats, as it will interfere with
            // the correct default behavior for those formats.

            Contract.Assert(primitive != null);

            object value = primitive.Value;

            // Only add an annotation if we want to override ODataLib's default type name serialization behavior.
            if (ShouldAddTypeNameAnnotation(metadataLevel))
            {
                string typeName;

                // Provide the type name to serialize (or null to force it not to serialize).
                if (ShouldSuppressTypeNameSerialization(value, metadataLevel))
                {
                    typeName = null;
                }
                else
                {
                    typeName = primitiveType.FullName();
                }

                primitive.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
                {
                    TypeName = typeName
                });
            }
        }

        private static bool ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel)
        {
            // Don't interfere with the correct default behavior in non-JSON light formats.
            // In all JSON light modes, take control of type name serialization.
            // For primitives (unlike other types), the default behavior does not matches the requirements for minimal
            // metadata mode, so the annotation is needed even in minimal metadata mode.
            return metadataLevel != ODataMetadataLevel.Default;
        }

        internal static ODataPrimitiveValue CreatePrimitive(object value, IEdmPrimitiveTypeReference primitveType,
            ODataMetadataLevel metadataLevel)
        {
            if (value == null)
            {
                return null;
            }

            object supportedValue = ConvertUnsupportedPrimitives(value);
            ODataPrimitiveValue primitive = new ODataPrimitiveValue(supportedValue);
            AddTypeNameAnnotationAsNeeded(primitive, primitveType, metadataLevel);
            return primitive;
        }

        internal static object ConvertUnsupportedPrimitives(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();

                // Note that type cannot be a nullable type as value is not null and it is boxed.
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                        return new String((char)value, 1);

                    case TypeCode.UInt16:
                        return (int)(ushort)value;

                    case TypeCode.UInt32:
                        return (long)(uint)value;

                    case TypeCode.UInt64:
                        return checked((long)(ulong)value);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new String(value as char[]);
                        }
                        else if (type == typeof(XElement))
                        {
                            return ((XElement)value).ToString();
                        }
                        else if (type == typeof(Binary))
                        {
                            return ((Binary)value).ToArray();
                        }
                        break;
                }
            }

            return value;
        }

        internal static bool CanTypeBeInferredInJson(object value)
        {
            Contract.Assert(value != null);

            TypeCode typeCode = Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                // The type for a Boolean, Int32 or String can always be inferred in JSON.
                case TypeCode.Boolean:
                case TypeCode.Int32:
                case TypeCode.String:
                    return true;
                // The type for a Double can be inferred in JSON ...
                case TypeCode.Double:
                    double doubleValue = (double)value;
                    // ... except for NaN or Infinity (positive or negative).
                    if (Double.IsNaN(doubleValue) || Double.IsInfinity(doubleValue))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }

        internal static bool ShouldSuppressTypeNameSerialization(object value, ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(metadataLevel != ODataMetadataLevel.Default);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.MinimalMetadata:
                    // Currently open properties are not supported, so the type for each property always appears in
                    // metadata.
                    const bool PropertyTypeAppearsInMetadata = true;
                    return PropertyTypeAppearsInMetadata;
                case ODataMetadataLevel.FullMetadata:
                default: // All values already specified; just keeping the compiler happy.
                    return CanTypeBeInferredInJson(value);
            }
        }
    }
}
