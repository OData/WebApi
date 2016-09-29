// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData enum types.
    /// </summary>
    public class ODataEnumDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEnumDeserializer"/> class.
        /// </summary>
        public ODataEnumDeserializer()
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
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            IEdmTypeReference edmType = readContext.GetEdmType(type);
            Contract.Assert(edmType != null);

            ODataProperty property = messageReader.ReadProperty();
            return ReadInline(property, edmType, readContext);
        }

        /// <inheritdoc />
        public override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            ODataProperty property = item as ODataProperty;
            if (property != null)
            {
                item = property.Value;
            }

            if (readContext.IsUntyped)
            {
                Contract.Assert(edmType.TypeKind() == EdmTypeKind.Enum);
                return new EdmEnumObject((IEdmEnumTypeReference)edmType, ((ODataEnumValue)item).Value);
            }

            Type clrType = EdmLibHelpers.GetClrType(edmType, readContext.Model);
            return EnumDeserializationHelpers.ConvertEnumValue(item, clrType);
        }
    }
}
