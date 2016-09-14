// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    internal static class ODataPayloadKindHelper
    {
        public static bool IsDefined(ODataPayloadKind payloadKind)
        {
            return payloadKind == ODataPayloadKind.Batch
                || payloadKind == ODataPayloadKind.BinaryValue
                || payloadKind == ODataPayloadKind.Collection
                || payloadKind == ODataPayloadKind.EntityReferenceLink
                || payloadKind == ODataPayloadKind.EntityReferenceLinks
                || payloadKind == ODataPayloadKind.Resource
                || payloadKind == ODataPayloadKind.Error
                || payloadKind == ODataPayloadKind.ResourceSet
                || payloadKind == ODataPayloadKind.MetadataDocument
                || payloadKind == ODataPayloadKind.Parameter
                || payloadKind == ODataPayloadKind.Property
                || payloadKind == ODataPayloadKind.ServiceDocument
                || payloadKind == ODataPayloadKind.Value
                || payloadKind == ODataPayloadKind.IndividualProperty
                || payloadKind == ODataPayloadKind.Delta
                || payloadKind == ODataPayloadKind.Asynchronous
                || payloadKind == ODataPayloadKind.Unsupported;
        }

        public static void Validate(ODataPayloadKind payloadKind, string parameterName)
        {
            if (!IsDefined(payloadKind))
            {
                throw Error.InvalidEnumArgument(parameterName, (int)payloadKind, typeof(ODataPayloadKind));
            }
        }
    }
}
