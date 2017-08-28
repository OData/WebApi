// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.AspNet.OData.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            Assert.Equal(JToken.Parse(expected), JToken.Parse(actual), JToken.EqualityComparer);
        }
    }
}
