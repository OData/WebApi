// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.AspNet.OData.Query
{
    public class HandleNullPropagationOptionHelperTest : EnumHelperTestBase<HandleNullPropagationOption>
    {
        public HandleNullPropagationOptionHelperTest()
            : base(HandleNullPropagationOptionHelper.IsDefined, HandleNullPropagationOptionHelper.Validate, (HandleNullPropagationOption)999)
        {
        }
    }
}
