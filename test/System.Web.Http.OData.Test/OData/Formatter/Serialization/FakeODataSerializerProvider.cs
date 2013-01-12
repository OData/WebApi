// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Serialization
{
    internal class FakeODataSerializerProvider : ODataSerializerProvider
    {
        private readonly ODataSerializer _serializer;

        public FakeODataSerializerProvider(ODataSerializer serializer)
        {
            _serializer = serializer;
        }

        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type)
        {
            return _serializer;
        }

        public override ODataSerializer CreateEdmTypeSerializer(IEdmTypeReference type)
        {
            return _serializer;
        }
    }
}
