﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http
{
    internal class ODataBatchRequestHelper
    {
        public static HttpMessageContent CreateODataRequestContent(HttpRequestMessage request)
        {
            var changeSetMessageContent = new HttpMessageContent(request);
            changeSetMessageContent.Headers.ContentType.Parameters.Clear();
            changeSetMessageContent.Headers.TryAddWithoutValidation("Content-Transfer-Encoding", "binary");
            return changeSetMessageContent;
        }
    }
}