// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataComplexTypeDeserializer : ODataEntryDeserializer<ODataComplexValue>
    {
        public ODataComplexTypeDeserializer(IEdmComplexTypeReference edmComplexType, ODataDeserializerProvider deserializerProvider)
            : base(edmComplexType, ODataPayloadKind.Property, deserializerProvider)
        {
            EdmComplexType = edmComplexType;
        }

        public IEdmComplexTypeReference EdmComplexType { get; private set; }

        public override object ReadInline(ODataComplexValue complexValue, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (complexValue == null)
            {
                return null;
            }

            RecurseEnter(readContext);

            object complexResource = CreateResource(EdmComplexType.ComplexDefinition(), readContext.Model);
            foreach (ODataProperty complexProperty in complexValue.Properties)
            {
                ApplyProperty(complexProperty, EdmComplexType, complexResource, DeserializerProvider, readContext);
            }

            RecurseLeave(readContext);

            return complexResource;
        }
    }
}
