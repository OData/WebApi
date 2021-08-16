//-----------------------------------------------------------------------------
// <copyright file="ODataUrlAssert.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public static class ODataUrlAssert
    {
        /// <summary>
        /// Comparing the OData URL is different from comparing normal URL
        /// 1) Base address part is compared case-insensitive
        /// 2) The remaining part is case-sensitive
        /// </summary>
        public static void UrlEquals(string expect, string actual, string baseAddress)
        {
            Assert.Equal(
                expect.ToLowerInvariant(),
                actual.ToLowerInvariant());

            Assert.Equal(
                expect.Substring(0, baseAddress.Length).ToLowerInvariant(),
                baseAddress.ToLowerInvariant());

            Assert.Equal(
                expect.Substring(baseAddress.Length),
                actual.Substring(baseAddress.Length));
        }

        /// <summary>
        /// Compare a OData url in a given JSON
        /// </summary>
        public static void UrlEquals(string expect, JObject json, string propertName, string baseAddress)
        {
            JToken token;

            if (json.TryGetValue(propertName, out token))
            {
                ODataUrlAssert.UrlEquals(expect, token.ToString(), baseAddress);
            }
            else
            {
                Assert.False(true, "Property " + propertName + " is not found in JSON object.");
            }
        }
    }
}
