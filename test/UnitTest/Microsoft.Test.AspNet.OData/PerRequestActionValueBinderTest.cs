// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.Tracing;
using Microsoft.AspNet.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
using Microsoft.Test.AspNet.OData.TestCommon.Models;
using Moq;
using Xunit;

namespace Microsoft.Test.AspNet.OData
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
