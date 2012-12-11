// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            expected = expected.Trim();
            actual = actual.Trim();

            // compare line by line since odata json typically differs from baseline by spaces
            string[] expectedLines = expected.Split('\n').ToList().ConvertAll((str) => str.Trim()).ToArray();
            string[] actualLines = actual.Split('\n').ToList().ConvertAll((str) => str.Trim()).ToArray();
            Assert.Equal(expectedLines, actualLines);
        }
    }
}
