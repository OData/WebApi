// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.PartialTrust
{
    public class BasicScenarioTest : MarshalByRefObject
    {
        [Fact]
        public void BasicSelfHostedEchoControllerWorks()
        {
            ScenarioHelper.RunTest(
                "Echo",
                "/{s}",
                new HttpRequestMessage(HttpMethod.Post, "http://localhost/Echo/foo"),
                (response) =>
                {
                    Assert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    Assert.Equal("foo", response.Content.ReadAsAsync<string>().Result);
                }
            );
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/xml")]
        public void SimpleConNegWorks(string mediaType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Echo/ContentNegotiatedEcho/foo");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            ScenarioHelper.RunTest(
                "Echo",
                "/{action}/{s}",
                request,
                (response) =>
                {
                    Assert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
                    Assert.Equal(mediaType, response.Content.Headers.ContentType.MediaType);
                }
            );
        }
    }

    [RunWith(typeof(PartialTrustRunner))]
    public class PartialTrustBasicScenarioTest : BasicScenarioTest { }

    public class EchoController : ApiController
    {
        public HttpResponseMessage Get(string s)
        {
            return new HttpResponseMessage() { Content = new StringContent(s) };
        }

        public string ContentNegotiatedEcho(string s)
        {
            return s;
        }
    }
}
