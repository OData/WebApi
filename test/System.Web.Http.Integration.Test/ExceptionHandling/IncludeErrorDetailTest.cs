// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http
{
    public class IncludeErrorDetailTest
    {
        public static TheoryDataSet ThrowingOnActionIncludesErrorDetailData
        {
            get
            {
                return new TheoryDataSet<bool, IncludeErrorDetailPolicy, bool>()
                {
                    // isLocal, includeErrorDetail, expectErrorDetail
                    { true, IncludeErrorDetailPolicy.LocalOnly, true },
                    { false, IncludeErrorDetailPolicy.LocalOnly, false },
                    { true, IncludeErrorDetailPolicy.Always, true },
                    { false, IncludeErrorDetailPolicy.Always, true },
                    { true, IncludeErrorDetailPolicy.Never, false },
                    { false, IncludeErrorDetailPolicy.Never, false }
                };
            }
        }

        [Theory]
        [PropertyData("ThrowingOnActionIncludesErrorDetailData")]
        public void ThrowingOnActionIncludesErrorDetail(bool isLocal, IncludeErrorDetailPolicy includeErrorDetail, bool expectErrorDetail)
        {
            string controllerName = "Exception";
            string requestUrl = String.Format("{0}/{1}/{2}", "http://www.foo.com", controllerName, "ArgumentNull");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Properties["MS_IsLocal"] = new Lazy<bool>(() => isLocal);

            ScenarioHelper.RunTest(
                controllerName,
                "/{action}",
                request,
                (response) =>
                {
                    if (expectErrorDetail)
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
                    config.IncludeErrorDetailPolicy = includeErrorDetail;
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
            JObject json = JToken.Parse(response.Content.ReadAsStringAsync().Result) as JObject;
            Assert.Equal(1, json.Count);
            string errorMessage = ((JValue)json["Message"]).ToString();
            Assert.Equal("An exception has occurred. For more information about the error, consider setting IncludeErrorDetailPolicy on your server's HttpConfiguration to Always.", errorMessage);
        }
    }
}