// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class ApiControllerActionInvokerTest
    {
        private readonly HttpActionContext _actionContext;
        private readonly Mock<HttpActionDescriptor> _actionDescriptorMock = new Mock<HttpActionDescriptor>();
        private readonly ApiControllerActionInvoker _actionInvoker = new ApiControllerActionInvoker();
        private readonly HttpControllerContext _controllerContext;
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly Mock<IActionResultConverter> _converterMock = new Mock<IActionResultConverter>();

        public ApiControllerActionInvokerTest()
        {
            _controllerContext = new HttpControllerContext()
            {
                Request = _request
            };
            _actionContext = new HttpActionContext(_controllerContext, _actionDescriptorMock.Object);
            _actionDescriptorMock.Setup(ad => ad.ResultConverter).Returns(_converterMock.Object);
        }

        [Fact]
        public void InvokeActionAsync_Cancels_IfCancellationTokenRequested()
        {
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            var response = _actionInvoker.InvokeActionAsync(ContextUtil.CreateActionContext(), cancellationSource.Token);

            Assert.Equal<TaskStatus>(TaskStatus.Canceled, response.Status);
        }

        [Fact]
        public void InvokeActionAsync_Throws_IfContextIsNull()
        {
            Assert.ThrowsArgumentNull(
                () => _actionInvoker.InvokeActionAsync(null, CancellationToken.None),
                "actionContext");
        }

        [Fact]
        public void InvokeActionAsync_InvokesActionDescriptorExecuteAsync()
        {
            var result = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            result.WaitUntilCompleted();
            _actionDescriptorMock.Verify(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments), Times.Once());
        }

        [Fact]
        public void InvokeActionAsync_PassesExecutionResultToConfiguredConverter()
        {
            var value = new object();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments))
                .Returns(TaskHelpers.FromResult(value));

            var result = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            result.WaitUntilCompleted();
            _converterMock.Verify(c => c.Convert(_actionContext.ControllerContext, value), Times.Once());
        }

        [Fact]
        public void InvokeActionAsync_ReturnsResponseFromConverter()
        {
            var response = new HttpResponseMessage();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(_actionContext.ControllerContext, _actionContext.ActionArguments))
                .Returns(TaskHelpers.FromResult(new object()));
            _converterMock.Setup(c => c.Convert(_actionContext.ControllerContext, It.IsAny<object>()))
                .Returns(response);

            var result = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            result.WaitUntilCompleted();
            Assert.Same(response, result.Result);
        }

        [Fact]
        public void InvokeActionAsync_WhenExecuteThrowsHttpResponseException_ReturnsResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(new HttpResponseException(response));

            var result = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            result.WaitUntilCompleted();
            Assert.Same(response, result.Result);
            Assert.Same(_request, result.Result.RequestMessage);
        }

        [Fact]
        public void InvokeActionAsync_WhenExecuteThrows_ReturnsFaultedTask()
        {
            Exception exception = new Exception();
            _actionDescriptorMock.Setup(ad => ad.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(exception);

            var result = _actionInvoker.InvokeActionAsync(_actionContext, CancellationToken.None);

            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.Same(exception, result.Exception.GetBaseException());
        }
    }
}
