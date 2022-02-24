//-----------------------------------------------------------------------------
// <copyright file="ODataPrimitiveSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
using System.Data.Linq;
#endif
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
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
        public override Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
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

            return messageWriter.WritePropertyAsync(CreateProperty(graph, edmType, writeContext.RootElementName, writeContext));
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
            // TODO: Bug 467598: validate the type of the object being passed in here with the underlying primitive type. 
            return CreatePrimitive(graph, primitiveType, writeContext);
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataPrimitiveValue primitive, IEdmPrimitiveTypeReference primitiveType,
            ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            Contract.Assert(primitive != null);

            object value = primitive.Value;
            string typeName = null; // Set null to force the type name not to serialize.

            // Provide the type name to serialize.
            if (!ShouldSuppressTypeNameSerialization(value, metadataLevel))
            {
                typeName = primitiveType.FullName();
            }

            if (typeName != null)
            {
                primitive.TypeAnnotation = new ODataTypeAnnotation(typeName);
            }
        }

        internal static ODataPrimitiveValue CreatePrimitive(object value, IEdmPrimitiveTypeReference primitiveType,
            ODataSerializerContext writeContext)
        {
            if (value == null)
            {
                return null;
            }

            object supportedValue = ConvertPrimitiveValue(value, primitiveType);
            ODataPrimitiveValue primitive = new ODataPrimitiveValue(supportedValue);

            if (writeContext != null)
            {
                AddTypeNameAnnotationAsNeeded(primitive, primitiveType, writeContext.MetadataLevel);
            }

            return primitive;
        }

        internal static object ConvertPrimitiveValue(object value, IEdmPrimitiveTypeReference primitiveType)
        {
            if (value == null)
            {
                return null;
            }

            Type type = value.GetType();

            if (primitiveType != null)
            {
                if (primitiveType.IsDate() && TypeHelper.IsDateTime(type))
                {
                    Date dt = (DateTime)value;
                    return dt;
                }

                if (primitiveType.IsTimeOfDay() && TypeHelper.IsTimeSpan(type))
                {
                    TimeOfDay tod = (TimeSpan)value;
                    return tod;
                }

                if (primitiveType is EdmDecimalTypeReference decimalTypeReference && decimalTypeReference.Scale.HasValue && value is decimal decimalValue)
                {
                    return CalculateScaleOfDecimal(decimalValue, decimalTypeReference.Scale.Value);
                }
            }

            return ConvertUnsupportedPrimitives(value);
        }

        static decimal CalculateScaleOfDecimal(decimal value, int scale)
        {
            string scaleForConverting = "0.";
            for (int i = 0; i < scale; i++)
            {
                scaleForConverting += "0";
            }
            return decimal.Parse(value.ToString(scaleForConverting, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
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

                    case TypeCode.DateTime:
                        DateTime dateTime = (DateTime)value;
                        return TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime);

                    default:
                        if (type == typeof(char[]))
                        {
                            return new String(value as char[]);
                        }
                        else if (type == typeof(XElement))
                        {
                            return ((XElement)value).ToString();
                        }
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
                        else if (type == typeof(Binary))
                        {
                            return ((Binary)value).ToArray();
                        }
#endif
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
            // For dynamic properties in minimal metadata level, the type name always appears as declared property.
            if (metadataLevel != ODataMetadataLevel.FullMetadata)
            {
                return true;
            }

            return CanTypeBeInferredInJson(value);
        }
    }
}
