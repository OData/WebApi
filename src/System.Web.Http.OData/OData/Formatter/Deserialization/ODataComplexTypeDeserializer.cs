// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataComplexTypeDeserializer : ODataEntryDeserializer
    {
        public ODataComplexTypeDeserializer(IEdmComplexTypeReference edmComplexType, ODataDeserializerProvider deserializerProvider)
            : base(edmComplexType, ODataPayloadKind.Property, deserializerProvider)
        {
            EdmComplexType = edmComplexType;
        }

        public IEdmComplexTypeReference EdmComplexType { get; private set; }

        public override object ReadInline(object item, ODataDeserializerContext readContext)
        {
            ODataComplexValue complexValue = item as ODataComplexValue;
            if (complexValue == null)
            {
                throw Error.Argument("item", SRResources.ItemMustBeOfType, typeof(ODataComplexValue).Name);
            }

            RecurseEnter(readContext);

            object complexResource = CreateResource(EdmComplexType.ComplexDefinition(), EdmModel);
            foreach (ODataProperty complexProperty in complexValue.Properties)
            {
                ApplyProperty(complexProperty, EdmComplexType, complexResource, DeserializerProvider, readContext);
            }

            RecurseLeave(readContext);

            return complexResource;
        }
    }
}
