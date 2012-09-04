// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class PatchKeyModeHelperTests : EnumHelperTestBase<PatchKeyMode>
    {
        public PatchKeyModeHelperTests()
            : base(PatchKeyModeHelper.IsDefined, PatchKeyModeHelper.Validate, (PatchKeyMode)555)
        {
        }
    }
}
