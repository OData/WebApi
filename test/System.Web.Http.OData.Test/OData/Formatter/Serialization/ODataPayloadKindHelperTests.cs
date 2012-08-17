// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.OData;
using Xunit;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataPayloadKindHelperTests : EnumHelperTestBase<ODataPayloadKind>
    {
        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(ODataPayloadKindHelper.IsDefined, ODataPayloadKind.Unsupported);
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Check_IsDefined_ReturnsFalseForUndefinedValues(ODataPayloadKindHelper.IsDefined, (ODataPayloadKind)999);
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Check_Validate_DoesNotThrowForDefinedValues(ODataPayloadKindHelper.Validate, ODataPayloadKind.Unsupported);
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Check_Validate_ThrowsForUndefinedValues(ODataPayloadKindHelper.Validate, (ODataPayloadKind)999);
        }
    }
}
