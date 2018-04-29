// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Globalization;
using System.Net.Http;

namespace Microsoft.AspNet.OData.Test.Batch
{
    internal class ODataBatchRequestHelper
    {
        public static HttpMessageContent CreateODataRequestContent(HttpRequestMessage request)
        {
            var changeSetMessageContent = new HttpMessageContent(request);
            changeSetMessageContent.Headers.ContentType.Parameters.Clear();
            changeSetMessageContent.Headers.TryAddWithoutValidation("Content-Transfer-Encoding", "binary");
            changeSetMessageContent.Headers.TryAddWithoutValidation(
                "Content-ID",
                Guid.NewGuid().GetHashCode().ToString(CultureInfo.InvariantCulture));
            return changeSetMessageContent;
        }
    }
}
#endif