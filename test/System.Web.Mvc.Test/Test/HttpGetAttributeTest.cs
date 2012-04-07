// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test
{
    public class HttpGetAttributeTest
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotPost()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpGetAttribute>("DELETE");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsPost()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpGetAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpGetAttribute>();
        }
    }
}
