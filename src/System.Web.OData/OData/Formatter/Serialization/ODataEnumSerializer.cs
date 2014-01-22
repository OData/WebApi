// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmEnumType" />'s.
    /// </summary>
    public class ODataEnumSerializer : ODataEdmTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataEnumSerializer"/>.
        /// </summary>
        public ODataEnumSerializer()
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
            if (!expectedType.IsEnum())
            {
                throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataEnumSerializer).Name, expectedType.FullName());
            }

            ODataEnumValue value = CreateODataEnumValue(graph, expectedType.AsEnum(), writeContext);
            if (value == null)
            {
                return new ODataNullValue();
            }

            return value;
        }

        /// <summary>
        /// Creates an <see cref="ODataEnumValue"/> for the object represented by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph">The enum value.</param>
        /// <param name="enumType">The EDM enum type of the value.</param>
        /// <param name="writeContext">The serializer write context.</param>
        /// <returns>The created <see cref="ODataEnumValue"/>.</returns>
        public virtual ODataEnumValue CreateODataEnumValue(object graph, IEdmEnumTypeReference enumType,
            ODataSerializerContext writeContext)
        {
            if (graph == null)
            {
                return null;
            }

            string value = null;
            if (graph.GetType().IsEnum)
            {
                value = graph.ToString();
            }

            ODataEnumValue enumValue = new ODataEnumValue(value, enumType.FullName());

            ODataMetadataLevel metadataLevel = writeContext != null
                ? writeContext.MetadataLevel
                : ODataMetadataLevel.Default;
            AddTypeNameAnnotationAsNeeded(enumValue, enumType, metadataLevel);

            return enumValue;
        }

        internal static void AddTypeNameAnnotationAsNeeded(ODataEnumValue enumValue, IEdmEnumTypeReference enumType, ODataMetadataLevel metadataLevel)
        {
            // ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
            // null when values should not be serialized. The TypeName property is different and should always be
            // provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
            // to serialize the type name (a null value prevents serialization).

            // Note that this annotation should not be used for Atom or JSON verbose formats, as it will interfere with
            // the correct default behavior for those formats.

            Contract.Assert(enumValue != null);

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
                    typeName = enumType.FullName();
                }

                enumValue.SetAnnotation(new SerializationTypeNameAnnotation
                {
                    TypeName = typeName
                });
            }
        }

        private static bool ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel)
        {
            switch (metadataLevel)
            {
                case ODataMetadataLevel.Default:
                case ODataMetadataLevel.MinimalMetadata:
                    return false;
                case ODataMetadataLevel.FullMetadata:
                case ODataMetadataLevel.NoMetadata:
                default:
                    return true;
            }
        }

        private static bool ShouldSuppressTypeNameSerialization(ODataMetadataLevel metadataLevel)
        {
            Contract.Assert(metadataLevel != ODataMetadataLevel.Default);
            Contract.Assert(metadataLevel != ODataMetadataLevel.MinimalMetadata);

            switch (metadataLevel)
            {
                case ODataMetadataLevel.NoMetadata:
                    return true;
                case ODataMetadataLevel.FullMetadata:
                default:
                    return false;
            }
        }
    }
}