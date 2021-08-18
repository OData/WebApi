//-----------------------------------------------------------------------------
// <copyright file="JsonAssert.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter
{
    internal static class JsonAssert
    {
        public static void Equal(string expected, string actual)
        {
            Assert.Equal(JToken.Parse(expected), JToken.Parse(actual), JToken.EqualityComparer);
        }
    }
}
