// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.SelfHost.Channels;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class HttpBindingSecurityModeHelperTest : EnumHelperTestBase<HttpBindingSecurityMode>
    {
        public HttpBindingSecurityModeHelperTest()
            : base(HttpBindingSecurityModeHelper.IsDefined, HttpBindingSecurityModeHelper.Validate, (HttpBindingSecurityMode)999)
        {
        }
    }
}
