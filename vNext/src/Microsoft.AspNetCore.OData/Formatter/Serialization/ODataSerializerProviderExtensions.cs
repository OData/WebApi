// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    internal static class ODataSerializerProviderExtensions
    {
        public static ODataEdmTypeSerializer GetEdmTypeSerializer(this IODataSerializerProvider serializerProvider,
            IEdmModel model, object instance, HttpContext context)
        {
            Contract.Assert(serializerProvider != null);
            Contract.Assert(model != null);
            Contract.Assert(instance != null);

            Contract.Assert(instance != null);

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                return serializerProvider.GetEdmTypeSerializer(context, edmObject.GetEdmType());
            }

            return serializerProvider.GetODataPayloadSerializer(context, instance.GetType()) as ODataEdmTypeSerializer;
        }
    }
}
