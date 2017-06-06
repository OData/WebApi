// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataPayloadKindHelperTests : EnumHelperTestBase<ODataPayloadKind>
    {
        public ODataPayloadKindHelperTests()
            : base(ODataPayloadKindHelper.IsDefined, ODataPayloadKindHelper.Validate, (ODataPayloadKind)999)
        {
        }
    }
}
