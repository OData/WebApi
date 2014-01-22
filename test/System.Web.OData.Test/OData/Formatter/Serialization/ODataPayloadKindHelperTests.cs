// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataPayloadKindHelperTests : EnumHelperTestBase<ODataPayloadKind>
    {
        public ODataPayloadKindHelperTests()
            : base(ODataPayloadKindHelper.IsDefined, ODataPayloadKindHelper.Validate, (ODataPayloadKind)999)
        {
        }
    }
}
