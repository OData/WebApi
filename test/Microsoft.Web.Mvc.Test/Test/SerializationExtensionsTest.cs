// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc;
using Microsoft.Web.UnitTestUtil;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Mvc.Test
{
    public class SerializationExtensionsTest
    {
        [Fact]
        public void SerializeFromProvidedValueOverridesViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary
            {
                { "someKey", 42 }
            };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Serialize("Hello!", SerializationMode.Signed)).Returns("some-value");

            // Act
            MvcHtmlString htmlString = helper.Serialize("someKey", "Hello!", SerializationMode.Signed, mockSerializer.Object);

            // Assert
            Assert.Equal(@"<input name=""someKey"" type=""hidden"" value=""some-value"" />", htmlString.ToHtmlString());
        }

        [Fact]
        public void SerializeFromViewData()
        {
            // Arrange
            ViewDataDictionary vdd = new ViewDataDictionary
            {
                { "someKey", 42 }
            };
            HtmlHelper helper = MvcHelper.GetHtmlHelper(vdd);

            Mock<MvcSerializer> mockSerializer = new Mock<MvcSerializer>();
            mockSerializer.Setup(o => o.Serialize(42, SerializationMode.EncryptedAndSigned)).Returns("some-other-value");

            // Act
            MvcHtmlString htmlString = helper.Serialize("someKey", SerializationMode.EncryptedAndSigned, mockSerializer.Object);

            // Assert
            Assert.Equal(@"<input name=""someKey"" type=""hidden"" value=""some-other-value"" />", htmlString.ToHtmlString());
        }

        [Fact]
        public void SerializeThrowsIfHtmlHelperIsNull()
        {
            Assert.ThrowsArgumentNull(
                delegate { SerializationExtensions.Serialize(null, "someName"); }, "htmlHelper");
        }

        [Fact]
        public void SerializeThrowsIfNameIsEmpty()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Serialize(""); }, "name");
        }

        [Fact]
        public void SerializeThrowsIfNameIsNull()
        {
            // Arrange
            HtmlHelper helper = MvcHelper.GetHtmlHelper(new ViewDataDictionary());

            Assert.ThrowsArgumentNullOrEmpty(
                delegate { helper.Serialize(null); }, "name");
        }
    }
}
