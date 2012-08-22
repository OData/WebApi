// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.Services
{
    class ServicesExtensionsTests
    {
        [Fact]
        public void Call_With_Null()
        {
            ControllerServices sc = null;

            Assert.ThrowsArgumentNull(() => { sc.GetValueProviderFactories(); }, "services");
        }
    }
}
