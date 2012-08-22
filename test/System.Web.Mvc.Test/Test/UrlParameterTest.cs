// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class UrlParameterTest
    {
        [Fact]
        public void UrlParameterOptionalToStringReturnsEmptyString()
        {
            // Act & Assert
            Assert.Empty(UrlParameter.Optional.ToString());
        }
    }
}
