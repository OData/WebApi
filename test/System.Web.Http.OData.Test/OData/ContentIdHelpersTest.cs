// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class ContentIdHelpersTest
    {
        [Theory]
        [InlineData("$1/Orders", "http://localhost/OData/Customers(42)/Orders")]
        [InlineData("http://localhost/$1/Orders(42)", "http://localhost/OData/Customers(42)/Orders(42)")]
        [InlineData("http://localhost/NoContentID", "http://localhost/NoContentID")]
        public void ResolveContentId_ResolvesContentIDInUrl(string url, string expectedResolvedUrl)
        {
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            contentIdToLocationMapping.Add("1", "http://localhost/OData/Customers(42)");

            string resolvedUrl = ContentIdHelpers.ResolveContentId(url, contentIdToLocationMapping);

            Assert.Equal(expectedResolvedUrl, resolvedUrl);
        }
    }
}
