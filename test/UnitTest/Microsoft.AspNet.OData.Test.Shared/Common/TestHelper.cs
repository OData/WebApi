//-----------------------------------------------------------------------------
// <copyright file="TestHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Common
{
    public static class UnitTestHelper
    {
        public static bool EnglishBuildAndOS
        {
            get
            {
                bool englishBuild = String.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, "en",
                                                  StringComparison.OrdinalIgnoreCase);
                bool englishOS = String.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, "en",
                                               StringComparison.OrdinalIgnoreCase);
                return englishBuild && englishOS;
            }
        }

        public static void AssertEqualsIgnoreWhitespace(string expected, string actual)
        {
            expected = new String(expected.Where(c => !Char.IsWhiteSpace(c)).ToArray());
            actual = new String(actual.Where(c => !Char.IsWhiteSpace(c)).ToArray());

            Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
        }
    }
}
