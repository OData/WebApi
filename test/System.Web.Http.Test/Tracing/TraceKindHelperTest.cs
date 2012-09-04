// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Tracing
{
    public class TraceKindHelperTest : EnumHelperTestBase<TraceKind>
    {
        public TraceKindHelperTest()
            : base(TraceKindHelper.IsDefined, TraceKindHelper.Validate, (TraceKind)999)
        {
        }
    }
}
