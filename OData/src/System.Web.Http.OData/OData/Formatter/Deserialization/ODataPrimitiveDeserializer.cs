// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData primitve types.
    /// </summary>
    public class ODataPrimitiveDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPrimitiveDeserializer"/> class.
        /// </summary>
        public ODataPrimitiveDeserializer()
            : base(ODataPayloadKind.Property)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            ODataProperty property = messageReader.ReadProperty();
            return ReadInline(property, edmType, readContext);
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            ODataProperty property = item as ODataProperty;
            if (property == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataProperty).Name);
            }

            return ReadPrimitive(property, readContext);
        }

        /// <summary>
        /// Deserializes the primitive from the given <paramref name="primitiveProperty"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="primitiveProperty">The primitive property to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized OData primitive value.</returns>
        public virtual object ReadPrimitive(ODataProperty primitiveProperty, ODataDeserializerContext readContext)
        {
            if (primitiveProperty == null)
            {
                throw Error.ArgumentNull("primitiveProperty");
            }

            return primitiveProperty.Value;
        }
    }
}
