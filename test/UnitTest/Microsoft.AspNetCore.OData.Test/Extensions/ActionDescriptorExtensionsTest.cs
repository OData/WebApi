// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if NETCOREAPP2_0
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Http.Internal;
#endif
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Moq;
using Xunit;
using ActionDescriptorExtensions = Microsoft.AspNet.OData.Extensions.ActionDescriptorExtensions;

namespace Microsoft.AspNet.OData.Test.Extensions
{

    public class ActionDescriptorExtensionsTest
    {
        [Fact]
        public void GetEdmModel_WillNotFail_With_Parallel_Calls()
        {
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(ApplicationPartManager))).Returns(new ApplicationPartManager());

#if NETCOREAPP2_0
            var request = new DefaultHttpRequest(new DefaultHttpContext { RequestServices = services.Object });
#else
            var request = new DefaultHttpContext { RequestServices = services.Object }.Request;
#endif
            var ad = new ActionDescriptor();
            Parallel.For(0, 10, i => { ActionDescriptorExtensions.GetEdmModel(ad, request, typeof(string)); });
        }
    }
}
