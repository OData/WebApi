// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Facebook.Authorization;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Mvc.Facebook.Test
{
    public class FacebookAuthorizeFilterTest
    {
        [Fact]
        public void OnAuthorization_ThrowsArgumentNullException()
        {
            FacebookConfiguration config = new FacebookConfiguration();
            FacebookAuthorizeFilter authorizeFilter = new FacebookAuthorizeFilter(config);
            Assert.ThrowsArgumentNull(
                () => authorizeFilter.OnAuthorization(null),
                "filterContext");
        }
    }
}
