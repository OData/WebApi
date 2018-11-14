using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
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
            var request = new DefaultHttpRequest(new DefaultHttpContext { RequestServices = services.Object });
            var ad = new ActionDescriptor();
            Parallel.For(0, 10, i => { ActionDescriptorExtensions.GetEdmModel(ad, request, typeof(string)); });
        }
    }
}
