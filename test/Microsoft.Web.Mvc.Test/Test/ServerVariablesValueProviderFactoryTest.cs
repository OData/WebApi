// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using Moq;
using Xunit;

namespace Microsoft.Web.Mvc.Test
{
    public class ServerVariablesValueProviderFactoryTest
    {
        [Fact]
        public void GetValueProvider()
        {
            // Arrange
            NameValueCollection serverVars = new NameValueCollection
            {
                { "foo", "fooValue" },
                { "bar.baz", "barBazValue" }
            };

            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.Setup(o => o.HttpContext.Request.ServerVariables).Returns(serverVars);

            ServerVariablesValueProviderFactory factory = new ServerVariablesValueProviderFactory();

            // Act
            IValueProvider provider = factory.GetValueProvider(mockControllerContext.Object);

            // Assert
            Assert.True(provider.ContainsPrefix("bar"));
            Assert.Equal("fooValue", provider.GetValue("foo").AttemptedValue);
            Assert.Equal(CultureInfo.InvariantCulture, provider.GetValue("foo").Culture);
        }
    }
}
