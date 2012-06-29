// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.SelfHost.Channels;
using Xunit;

namespace System.Net.Http.Formatting
{
    public class HttpBindingSecurityModeHelperTest : EnumHelperTestBase<HttpBindingSecurityMode>
    {
        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(HttpBindingSecurityModeHelper.IsDefined);
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Check_IsDefined_ReturnsFalseForUndefinedValues(HttpBindingSecurityModeHelper.IsDefined, (HttpBindingSecurityMode)999);
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Check_Validate_DoesNotThrowForDefinedValues(HttpBindingSecurityModeHelper.Validate);
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Check_Validate_ThrowsForUndefinedValues(HttpBindingSecurityModeHelper.Validate, (HttpBindingSecurityMode)999);
        }
    }
}
