// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Net.Http.Formatting
{
    public class StringComparisonHelperTest : EnumHelperTestBase<StringComparison>
    {
        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(StringComparisonHelper.IsDefined);
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Check_IsDefined_ReturnsFalseForUndefinedValues(StringComparisonHelper.IsDefined, (StringComparison)999);
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Check_Validate_DoesNotThrowForDefinedValues(StringComparisonHelper.Validate);
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Check_Validate_ThrowsForUndefinedValues(StringComparisonHelper.Validate, (StringComparison)999);
        }
    }
}
