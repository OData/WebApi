// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class StringComparisonHelperTest : EnumHelperTestBase<StringComparison>
    {
        public StringComparisonHelperTest()
            : base(StringComparisonHelper.IsDefined, StringComparisonHelper.Validate, (StringComparison)999)
        {
        }
    }
}
