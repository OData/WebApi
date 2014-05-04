// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Serialization
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
                || payloadKind == ODataPayloadKind.Entry
                || payloadKind == ODataPayloadKind.Error
                || payloadKind == ODataPayloadKind.Feed
                || payloadKind == ODataPayloadKind.MetadataDocument
                || payloadKind == ODataPayloadKind.Parameter
                || payloadKind == ODataPayloadKind.Property
                || payloadKind == ODataPayloadKind.ServiceDocument
                || payloadKind == ODataPayloadKind.Value
                || payloadKind == ODataPayloadKind.IndividualProperty
                || payloadKind == ODataPayloadKind.Delta
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
