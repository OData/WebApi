// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

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
