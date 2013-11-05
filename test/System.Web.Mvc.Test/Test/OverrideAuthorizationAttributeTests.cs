// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Filters;

namespace System.Web.Mvc.Test
{
    public class OverrideAuthorizationAttributeTests : OverrideFiltersAttributeTests
    {
        protected override Type ExpectedFiltersToOverride
        {
            get { return typeof(IAuthorizationFilter); }
        }
        
        protected override Type ProductUnderTestType
        {
            get { return typeof(OverrideAuthorizationAttribute); }
        }

        protected override IOverrideFilter CreateProductUnderTest()
        {
            return new OverrideAuthorizationAttribute();
        }
    }
}
