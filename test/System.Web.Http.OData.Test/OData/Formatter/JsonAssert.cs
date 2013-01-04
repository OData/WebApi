// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            // For now, simply compare the exact strings. Note that this approach requires whitespace to match exactly.
            Assert.Equal(expected, actual);
        }
    }
}
