//-----------------------------------------------------------------------------
// <copyright file="ODataEnumDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
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

            ODataProperty property = await messageReader.ReadPropertyAsync(edmType);
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

            IEdmEnumTypeReference enumTypeReference = edmType.AsEnum();
            ODataEnumValue enumValue = item as ODataEnumValue;
            if (readContext.IsUntyped)
            {
                Contract.Assert(edmType.TypeKind() == EdmTypeKind.Enum);
                return new EdmEnumObject(enumTypeReference, enumValue.Value);
            }

            IEdmEnumType enumType = enumTypeReference.EnumDefinition();

            // Enum member supports model alias case. So, try to use the Edm member name to retrieve the Enum value.
            var memberMapAnnotation = readContext.Model.GetClrEnumMemberAnnotation(enumType);
            if (memberMapAnnotation != null)
            {
                if (enumValue != null)
                {
                    IEdmEnumMember enumMember = enumType.Members.FirstOrDefault(m => m.Name == enumValue.Value);
                    if (enumMember != null)
                    {
                        var clrMember = memberMapAnnotation.GetClrEnumMember(enumMember);
                        if (clrMember != null)
                        {
                            return clrMember;
                        }
                    }
                }
            }

            Type clrType = EdmLibHelpers.GetClrType(edmType, readContext.Model);
            return EnumDeserializationHelpers.ConvertEnumValue(item, clrType);
        }
    }
}
