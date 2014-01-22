// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
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
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            Type clrType = EdmLibHelpers.GetClrType(edmType, readContext.Model);
            return EnumDeserializationHelpers.ConvertEnumValue(item, clrType);
        }
    }
}