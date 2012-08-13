// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        public DefaultODataDeserializerProvider(IEdmModel edmModel)
            : base(edmModel)
        {
        }

        protected override ODataEntryDeserializer CreateDeserializer(IEdmTypeReference edmType)
        {
            if (edmType != null)
            {
                switch (edmType.TypeKind())
                {
                    case EdmTypeKind.Entity:
                        return new ODataEntityDeserializer(edmType.AsEntity(), this);

                    case EdmTypeKind.Primitive:
                        return new ODataRawValueDeserializer(edmType.AsPrimitive());

                    case EdmTypeKind.Complex:
                        return new ODataComplexTypeDeserializer(edmType.AsComplex(), this);
                }
            }

            return null;
        }

        public override ODataDeserializer GetODataDeserializer(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (type == typeof(Uri))
            {
                return new ODataEntityReferenceLinkDeserializer();
            }

            IEdmTypeReference edmType = EdmModel.GetEdmTypeReference(type);
            if (edmType == null)
            {
                return null;
            }
            else
            {
                return GetODataDeserializer(edmType);
            }
        }
    }
}
