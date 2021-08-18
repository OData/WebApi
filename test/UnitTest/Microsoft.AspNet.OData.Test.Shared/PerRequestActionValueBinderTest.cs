//-----------------------------------------------------------------------------
// <copyright file="PerRequestActionValueBinderTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.Tracing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class PerRequestActionValueBinderTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_InnerActionValueBinder()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new PerRequestActionValueBinder(innerActionValueBinder: null),
                "innerActionValueBinder");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetBinding_Wraps_FormatterParameterBinding(bool tracingEnabled)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(new MockAssembly(typeof(PerRequestActionValueBinderTestSampleController))));

            if (tracingEnabled)
            {
                config.Services.Replace(typeof(ITraceWriter), new Mock<ITraceWriter>().Object);
                ITraceManager traceManager = config.Services.GetTraceManager();
                traceManager.Initialize(config);
            }

            IHttpControllerSelector controllerSelector = config.Services.GetHttpControllerSelector();
            IHttpActionSelector actionSelector = config.Services.GetActionSelector();

            HttpControllerDescriptor controllerDescriptor = controllerSelector.GetControllerMapping()["PerRequestActionValueBinderTestSample"];
            HttpActionDescriptor actionDescriptor = actionSelector.GetActionMapping(controllerDescriptor)["Post"].Single();

            PerRequestActionValueBinder binder = new PerRequestActionValueBinder(new DefaultActionValueBinder());

            // Act
            HttpActionBinding binding = binder.GetBinding(actionDescriptor);

            // Assert
            HttpParameterBinding parameterBinding = binding.ParameterBindings.Where(p => p.Descriptor.ParameterName == "customer").Single();
            Assert.True(parameterBinding is PerRequestParameterBinding);
        }
    }

    public class PerRequestActionValueBinderTestSampleController : ODataController
    {
        public void Post(Customer customer)
        {
            // NOOP
        }
    }
}
#endif
