// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerTests
    {
        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.HandleAsync(context, cancellationToken), "context");
        }

        [Fact]
        public void HandleAsync_IfExceptionIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>(MockBehavior.Strict);
            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = CreateContext(CreateExceptionContext());
            Assert.Null(context.ExceptionContext.Exception); // Guard
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                "ExceptionContext.Exception must not be null.");
        }

        [Fact]
        public void HandleAsync_IfShouldHandleReturnsTrue_DelegatesToHandleAsyncCore()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>(MockBehavior.Strict);
            Task expectedTask = CreateCompletedTask();
            mock.Setup(h => h.ShouldHandle(It.IsAny<ExceptionHandlerContext>())).Returns(true);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(expectedTask);

            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateValidContext();

            using (CancellationTokenSource tokenSource = CreateTokenSource())
            {
                CancellationToken expectedCancellationToken = tokenSource.Token;

                // Act
                Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

                // Assert
                Assert.Same(expectedTask, task);
                mock.Verify(h => h.ShouldHandle(expectedContext), Times.Once());
                mock.Verify(h => h.HandleAsync(expectedContext, expectedCancellationToken), Times.Once());
            }
        }

        [Fact]
        public void HandleAsync_IfShouldHandleReturnsFalse_ReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>(MockBehavior.Strict);
            Task expectedTask = CreateCompletedTask();
            mock.Setup(h => h.ShouldHandle(It.IsAny<ExceptionHandlerContext>())).Returns(false);

            IExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateValidContext();
            CancellationToken expectedCancellationToken = CancellationToken.None;

            // Act
            Task task = product.HandleAsync(expectedContext, expectedCancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            mock.Verify(h => h.ShouldHandle(expectedContext), Times.Once());
            mock.Verify(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Fact]
        public void HandleAsyncCore_DelegatesToHandleCore_AndReturnsCompletedTask()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext expectedContext = CreateContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            Task task = product.HandleAsync(expectedContext, cancellationToken);

            // Assert
            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);

            mock.Verify(h => h.Handle(expectedContext), Times.Once());
        }

        [Fact]
        public void HandleCore_DoesNotThrow()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = CreateContext();

            // Act & Assert
            Assert.DoesNotThrow(() => product.Handle(context));
        }

        [Fact]
        public void ShouldHandle_IfContextIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.ShouldHandle(context), "context");
        }

        [Fact]
        public void ShouldHandle_IfCallStackIsNull_Throws()
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = new ExceptionHandlerContext(new ExceptionContext());
            Assert.Null(context.ExceptionContext.CatchBlock); // Guard

            // Act & Assert
            Assert.ThrowsArgument(() => product.ShouldHandle(context), "context",
                "ExceptionContext.CatchBlock must not be null.");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldHandle_ReturnsIsTopLevelCatchBlock(bool isTopLevelCatchBlock)
        {
            // Arrange
            Mock<ExceptionHandler> mock = new Mock<ExceptionHandler>();
            mock.CallBase = true;
            ExceptionHandler product = mock.Object;

            ExceptionHandlerContext context = CreateContext(new ExceptionContext
            {
                CatchBlock = new ExceptionContextCatchBlock("IgnoreCaughtAt", isTopLevelCatchBlock, callsHandler: false)
            });

            // Act
            bool shouldHandle = product.ShouldHandle(context);

            // Assert
            Assert.Equal(isTopLevelCatchBlock, shouldHandle);
        }

        private static Task CreateCompletedTask()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            return source.Task;
        }

        private static ExceptionHandlerContext CreateContext()
        {
            return CreateContext(CreateExceptionContext());
        }

        private static ExceptionHandlerContext CreateContext(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }

        private static ExceptionContext CreateExceptionContext()
        {
            return new ExceptionContext();
        }

        private static CancellationTokenSource CreateTokenSource()
        {
            return new CancellationTokenSource();
        }

        private static ExceptionHandlerContext CreateValidContext()
        {
            return new ExceptionHandlerContext(new ExceptionContext
            {
                Exception = new Exception()
            });
        }
    }
}
