// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Filters;

namespace System.Web.Mvc.Test
{
    public class OverrideResultFiltersAttributeTests : OverrideFiltersAttributeTests
    {
        protected override Type ExpectedFiltersToOverride
        {
            get { return typeof(IResultFilter); }
        }
        
        protected override Type ProductUnderTestType
        {
            get { return typeof(OverrideResultFiltersAttribute); }
        }

        protected override IOverrideFilter CreateProductUnderTest()
        {
            return new OverrideResultFiltersAttribute();
        }
    }
}
