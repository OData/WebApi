// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
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
            enumValue.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
            {
                TypeName = null
            });

            return enumValue;
        }
    }
}