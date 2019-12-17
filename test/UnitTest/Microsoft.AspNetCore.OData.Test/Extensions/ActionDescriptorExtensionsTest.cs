using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if NETCOREAPP3_0
#else
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

#if NETCOREAPP3_0
            var request = new DefaultHttpContext { RequestServices = services.Object }.Request;
#else
            var request = new DefaultHttpRequest(new DefaultHttpContext { RequestServices = services.Object });
#endif
            var ad = new ActionDescriptor();
            Parallel.For(0, 10, i => { ActionDescriptorExtensions.GetEdmModel(ad, request, typeof(string)); });
        }
    }
}
