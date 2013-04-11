// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class RawValueSerializerProvider : ODataSerializerProvider
    {
        private readonly ODataSerializer _rawValueSerializer = new ODataRawValueSerializer();
        
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            IEdmPrimitiveType edmType = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type);
            return edmType == null ? null : _rawValueSerializer;
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            return null;
        }
    }
}
