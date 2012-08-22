// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class HostNameComparisonModeHelperTest : EnumHelperTestBase<HostNameComparisonMode>
    {
        public HostNameComparisonModeHelperTest()
            : base(HostNameComparisonModeHelper.IsDefined, HostNameComparisonModeHelper.Validate, (HostNameComparisonMode)999)
        {
        }
    }
}
