// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel;
using Xunit;

namespace System.Net.Http.Formatting
{
    public class HostNameComparisonModeHelperTest : EnumHelperTestBase<HostNameComparisonMode>
    {
        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(HostNameComparisonModeHelper.IsDefined);
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Check_IsDefined_ReturnsFalseForUndefinedValues(HostNameComparisonModeHelper.IsDefined, (HostNameComparisonMode)999);
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Check_Validate_DoesNotThrowForDefinedValues(HostNameComparisonModeHelper.Validate);
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Check_Validate_ThrowsForUndefinedValues(HostNameComparisonModeHelper.Validate, (HostNameComparisonMode)999);
        }
    }
}
