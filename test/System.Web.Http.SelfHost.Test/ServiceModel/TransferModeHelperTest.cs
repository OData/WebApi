// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class TransferModeHelperTest : EnumHelperTestBase<TransferMode>
    {
        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(TransferModeHelper.IsDefined);
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Check_IsDefined_ReturnsFalseForUndefinedValues(TransferModeHelper.IsDefined, (TransferMode)999);
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Check_Validate_DoesNotThrowForDefinedValues(TransferModeHelper.Validate);
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Check_Validate_ThrowsForUndefinedValues(TransferModeHelper.Validate, (TransferMode)999);
        }
    }
}
