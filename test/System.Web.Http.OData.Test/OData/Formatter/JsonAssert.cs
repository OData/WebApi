// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            // Due to a problem with one build system, don't assume source files use Environment.NewLine (they may just
            // use \n instead). Normalize the expected result to use Environment.NewLine.
            expected = expected.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine);

            // For now, simply compare the exact strings. Note that this approach requires whitespace to match exactly
            // (except for line endings).
            Assert.Equal(expected, actual);
        }
    }
}
