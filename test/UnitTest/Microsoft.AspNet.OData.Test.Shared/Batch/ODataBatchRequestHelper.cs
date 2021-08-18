//-----------------------------------------------------------------------------
// <copyright file="ODataBatchRequestHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
