// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class MessageResultTests
    {
        [Fact]
        public void Constructor_Throws_WhenResponseIsNull()
        {
            // Arrange, Act & Assert
            Assert.ThrowsArgumentNull(() => { new MessageResult(null); }, "response");
        }

        [Fact]
        public void Response_Returns_InstanceProvided()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                MessageResult result = new MessageResult(expectedResponse);

                // Act
                HttpResponseMessage response = result.Response;

                // Assert
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public void ExecuteAsync_Returns_InstanceProvided()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                IHttpActionResult result = new MessageResult(expectedResponse);

                // Act
                Task<HttpResponseMessage> task = result.ExecuteAsync(CancellationToken.None);

                // Assert
                Assert.NotNull(task);
                task.WaitUntilCompleted();

                using (HttpResponseMessage response = task.Result)
                {
                    Assert.Same(expectedResponse, response);
                }
            }
        }

        [Fact]
        public void ApiControllerMessage_CreatesCorrectMessageResult()
        {
            // Arrange
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                ApiController controller = CreateController();

                // Act
                MessageResult result = controller.Message(expectedResponse);

                // Assert
                Assert.NotNull(result);
                Assert.Same(expectedResponse, result.Response);
            }
        }

        private static ApiController CreateController()
        {
            return new FakeController();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private class FakeController : ApiController
        {
        }
    }
}
