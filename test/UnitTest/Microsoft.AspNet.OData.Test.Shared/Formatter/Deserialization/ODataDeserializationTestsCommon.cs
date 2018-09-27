// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    internal class ODataDeserializationTestsCommon
    {

        internal static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        internal static IODataRequestMessage GetODataMessage(string content)
        {
            // While NetCore does not use this for AspNet, it can be used here to create
            // an HttpRequestODataMessage, which is a Test type that implments IODataRequestMessage
            // wrapped around an HttpRequestMessage.
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), "http://localhost/OData/Suppliers(1)/Address");

            request.Content = new StringContent(content);
            request.Headers.Add("OData-Version", "4.0");

            MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            request.Headers.Accept.Add(mediaType);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpRequestODataMessage(request);
        }
    }
}
