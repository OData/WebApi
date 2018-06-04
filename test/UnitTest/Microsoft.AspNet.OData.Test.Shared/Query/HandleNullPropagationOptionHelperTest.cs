// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Common;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class HandleNullPropagationOptionHelperTest : EnumHelperTestBase<HandleNullPropagationOption>
    {
        public HandleNullPropagationOptionHelperTest()
            : base(HandleNullPropagationOptionHelper.IsDefined, HandleNullPropagationOptionHelper.Validate, (HandleNullPropagationOption)999)
        {
        }
    }
}
