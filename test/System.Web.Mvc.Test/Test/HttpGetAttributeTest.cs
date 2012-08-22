// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

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
