// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test {
    public class HttpPatchAttributeTest 
    {
        [Fact]
        public void IsValidForRequestReturnsFalseIfHttpVerbIsNotPatch() 
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithInvalidVerb<HttpPatchAttribute>("GET");
        }

        [Fact]
        public void IsValidForRequestReturnsTrueIfHttpVerbIsPatch() 
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeWithValidVerb<HttpPatchAttribute>("PATCH");
        }

        [Fact]
        public void IsValidForRequestThrowsIfControllerContextIsNull()
        {
            HttpVerbAttributeHelper.TestHttpVerbAttributeNullControllerContext<HttpPatchAttribute>();
        }
    }
}