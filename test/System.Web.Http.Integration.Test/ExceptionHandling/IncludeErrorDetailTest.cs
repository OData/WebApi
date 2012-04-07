// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http
{
    public class IncludeErrorDetailTest
    {
        public static IEnumerable<object[]> Data
        {
            get
            {
                return new object[][]
                {
                    new object[] { "localhost", null, true },
                    new object[] { "127.0.0.1", null, true },
                    new object[] { "www.foo.com", null, false },
                    new object[] { "localhost", IncludeErrorDetailPolicy.LocalOnly, true },
                    new object[] { "www.foo.com", IncludeErrorDetailPolicy.LocalOnly, false },
                    new object[] { "localhost", IncludeErrorDetailPolicy.Always, true },
                    new object[] { "www.foo.com", IncludeErrorDetailPolicy.Always, true },
                    new object[] { "localhost", IncludeErrorDetailPolicy.Never, false },
                    new object[] { "www.foo.com", IncludeErrorDetailPolicy.Never, false }
                };
            }
        }

        [Theory]
        [PropertyData("Data")]
        public void ThrowingOnActionIncludesErrorDetail(string hostName, IncludeErrorDetailPolicy? includeErrorDetail, bool shouldIncludeErrorDetail)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", "http://" + hostName, controllerName, "ArgumentNull");
            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                new HttpRequestMessage(HttpMethod.Post, requestUrl),
                (response) =>
                {
                    if (shouldIncludeErrorDetail)
                    {
                        AssertResponseIncludesErrorDetail(response);
                    }
                    else
                    {
                        AssertResponseDoesNotIncludeErrorDetail(response);
                    }
                },
                (config) =>
                {
                    if (includeErrorDetail.HasValue)
                    {
                        config.IncludeErrorDetailPolicy = includeErrorDetail.Value;
                    }
                }
            );
        }

        private void AssertResponseIncludesErrorDetail(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            dynamic json = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            string result = json.ExceptionType;
            Assert.Equal(typeof(ArgumentNullException).FullName, result);
        }

        private void AssertResponseDoesNotIncludeErrorDetail(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Null(response.Content);
        }
    }
}