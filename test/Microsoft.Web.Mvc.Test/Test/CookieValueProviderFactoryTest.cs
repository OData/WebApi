// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Moq;
using Xunit;

namespace Microsoft.Web.Mvc.Test
{
    public class CookieValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            HttpCookieCollection cookies = new HttpCookieCollection
            {
                new HttpCookie("foo", "fooValue"),
                new HttpCookie("bar.baz", "barBazValue"),
                new HttpCookie("", "emptyValue"),
                new HttpCookie(null, "nullValue")
            };

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.Cookies).Returns(cookies);

            CookieValueProviderFactory factory = new CookieValueProviderFactory();

            // Act
            IValueProvider provider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.Null(provider.GetValue(""));
            Assert.True(provider.ContainsPrefix("bar"));
            Assert.Equal("fooValue", provider.GetValue("foo").AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, provider.GetValue("foo").Culture);
        }
    }
}
