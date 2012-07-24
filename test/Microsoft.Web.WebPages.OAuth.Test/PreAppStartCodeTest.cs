// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


using System.Web.WebPages.TestUtils;
using Xunit;

namespace Microsoft.Web.WebPages.OAuth.Test
{
    public class PreAppStartCodeTest
    {
        [Fact]
        public void TestPreAppStartClass()
        {
            PreAppStartTestHelper.TestPreAppStartClass(typeof(PreApplicationStartCode));
        }
    }
}
