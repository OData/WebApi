// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
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

            ODataProperty property = messageReader.ReadProperty(edmType);
            return ReadInline(property, edmType, readContext);
        }
        
        /// <inheritdoc />
        public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            ODataProperty property = await messageReader.ReadPropertyAsync(edmType);
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

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            //Try and change the value appropriately if type is specified
            if (readContext.ResourceType != null && primitiveProperty.Value != null)
            {
                return EdmPrimitiveHelpers.ConvertPrimitiveValue(primitiveProperty.Value, readContext.ResourceType);
            }

            return primitiveProperty.Value;
        }
    }
}
