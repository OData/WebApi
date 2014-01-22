// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            Assert.Equal(JToken.Parse(expected), JToken.Parse(actual), JToken.EqualityComparer);
        }
    }
}
