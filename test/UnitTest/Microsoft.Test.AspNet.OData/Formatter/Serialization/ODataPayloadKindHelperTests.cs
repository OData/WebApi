// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData.Formatter.Serialization
{
    public class ODataPayloadKindHelperTests : EnumHelperTestBase<ODataPayloadKind>
    {
        public ODataPayloadKindHelperTests()
            : base(ODataPayloadKindHelper.IsDefined, ODataPayloadKindHelper.Validate, (ODataPayloadKind)999)
        {
        }
    }
}
