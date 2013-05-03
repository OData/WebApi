// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.TestCommon;

namespace System.Web.Http.Owin
{
    public class OwinHttpRequestMessageExtensionsTest
    {
        [Fact]
        public void GetOwinEnvironment_ReturnsNull_WhenNotSet()
        {
            Assert.Null(new HttpRequestMessage().GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinEnvironment_ReturnsSetEnvironment()
        {
            var request = new HttpRequestMessage();
            var environment = new Dictionary<string, object>();
            request.SetOwinEnvironment(environment);

            Assert.Equal(environment, request.GetOwinEnvironment());
        }

        [Fact]
        public void GetOwinRequest_ReturnsRequestForOwinEnvironment()
        {
            // Arrange
            IDictionary<string, object> expectedEnvironment = new Dictionary<string, object>();

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.SetOwinEnvironment(expectedEnvironment);

                // Act
                OwinRequest owinRequest = request.GetOwinRequest();

                // Assert
                Assert.Same(expectedEnvironment, owinRequest.Environment);
            }
        }

        [Fact]
        public void GetOwinRequest_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.GetOwinRequest(null); }, "request");
        }

        [Fact]
        public void GetOwinRequest_Throws_WhenOwinEnvironmentNotSet()
        {
            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => { request.GetOwinRequest(); },
                    "No OWIN environment is available for the request.");
            }
        }

        [Fact]
        public void GetOwinResponse_ReturnsResponseForOwinEnvironment()
        {
            // Arrange
            IDictionary<string, object> expectedEnvironment = new Dictionary<string, object>();

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.SetOwinEnvironment(expectedEnvironment);

                // Act
                OwinResponse response = request.GetOwinResponse();

                // Assert
                Assert.Same(expectedEnvironment, response.Environment);
            }
        }

        [Fact]
        public void GetOwinResponse_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(() => { OwinHttpRequestMessageExtensions.GetOwinResponse(null); }, "request");
        }

        [Fact]
        public void GetOwinResponse_Throws_WhenOwinEnvironmentNotSet()
        {
            // Arrange
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act & Assert
                Assert.Throws<InvalidOperationException>(() => { request.GetOwinResponse(); },
                    "No OWIN environment is available for the request.");
            }
        }
    }
}
