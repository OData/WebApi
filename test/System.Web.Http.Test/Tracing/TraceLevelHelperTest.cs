// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    public class TraceLevelHelperTest : EnumHelperTestBase<TraceLevel>
    {
        public TraceLevelHelperTest()
            : base(TraceLevelHelper.IsDefined, TraceLevelHelper.Validate, (TraceLevel)999)
        {
        }
    }
}
