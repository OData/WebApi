// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    internal static class ODataSerializerProviderExtensions
    {
        public static ODataEdmTypeSerializer GetEdmTypeSerializer(this ODataSerializerProvider serializerProvider,
            object instance, HttpRequestMessage request)
        {
            Contract.Assert(serializerProvider != null);
            Contract.Assert(instance != null);

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                return serializerProvider.GetEdmTypeSerializer(edmObject.GetEdmType());
            }

            return serializerProvider.GetODataPayloadSerializer(instance.GetType(), request) as ODataEdmTypeSerializer;
        }
    }
}
