//-----------------------------------------------------------------------------
// <copyright file="HandleNullPropagationOptionHelperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
