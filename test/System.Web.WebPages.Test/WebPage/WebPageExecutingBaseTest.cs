// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Web.WebPages.Instrumentation;
using Moq;
using Xunit;

namespace System.Web.WebPages.Test
{
    public class WebPageExecutingBaseTest
    {
        [Fact]
        public void NormalizeLayoutPageUsesVirtualPathFactoryManagerToDetermineIfLayoutFileExists()
        {
            // Arrange
            var layoutPagePath = "~/sitelayout.cshtml";
            var layoutPage = Utils.CreatePage(null, layoutPagePath);
            var page = Utils.CreatePage(null);
            var objectFactory = new Mock<IVirtualPathFactory>();
            objectFactory.Setup(c => c.Exists(It.Is<string>(p => p.Equals(layoutPagePath)))).Returns(true).Verifiable();
            page.VirtualPathFactory = objectFactory.Object;

            // Act
            var path = page.NormalizeLayoutPagePath(layoutPage.VirtualPath);

            // Assert
            objectFactory.Verify();
            Assert.Equal(path, layoutPage.VirtualPath);
        }

        [Fact]
        public void NormalizeLayoutPageAcceptsRelativePathsToLayoutPage()
        {
            // Arrange
            var page = Utils.CreatePage(null, "~/dir/default.cshtml");
            var layoutPage = Utils.CreatePage(null, "~/layouts/sitelayout.cshtml");
            var objectFactory = new HashVirtualPathFactory(page, layoutPage);
            page.VirtualPathFactory = objectFactory;

            // Act
            var path = page.NormalizeLayoutPagePath(@"../layouts/sitelayout.cshtml");

            // Assert
            Assert.Equal(path, layoutPage.VirtualPath);
        }

        [Fact]
        public void BeginContextSilentlyFailsIfInstrumentationIsNotAvailable()
        {
            // Arrange
            bool called = false;

            var pageMock = new Mock<WebPageExecutingBase>() { CallBase = true };
            pageMock.Setup(p => p.Context).Returns(new Mock<HttpContextBase>().Object);
            pageMock.Object.InstrumentationService.IsAvailable = false;
            pageMock.Object.InstrumentationService.ExtractInstrumentationService = c =>
            {
                called = true;
                return null;
            };

            // Act
            pageMock.Object.BeginContext("~/dir/default.cshtml", 0, 1, true);

            // Assert
            Assert.False(called);
        }

        [Fact]
        public void EndContextSilentlyFailsIfInstrumentationIsNotAvailable()
        {
            // Arrange
            bool called = false;

            var pageMock = new Mock<WebPageExecutingBase>() { CallBase = true };
            pageMock.Setup(p => p.Context).Returns(new Mock<HttpContextBase>().Object);
            pageMock.Object.InstrumentationService.IsAvailable = false;
            pageMock.Object.InstrumentationService.ExtractInstrumentationService = c =>
            {
                called = true;
                return null;
            };

            // Act
            pageMock.Object.EndContext("~/dir/default.cshtml", 0, 1, true);

            // Assert
            Assert.False(called);
        }

        [Fact]
        public void WriteAttributeToWritesAttributeNormallyIfNoValuesSpecified()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                expected: " alt=\"\"");
        }

        [Fact]
        public void WriteAttributeToWritesNothingIfSingleNullValueProvided()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                values: new[] {
                    new AttributeValue(new PositionTagged<string>(String.Empty, 142), new PositionTagged<object>(null, 124), literal: true)
                },
                expected: "");
        }

        [Fact]
        public void WriteAttributeToWritesNothingIfSingleFalseValueProvided()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                values: new[] {
                    new AttributeValue(new PositionTagged<string>(String.Empty, 142), new PositionTagged<object>(false, 124), literal: true)
                },
                expected: "");
        }

        [Fact]
        public void WriteAttributeToWritesGlobalPrefixIfSingleValueProvided()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                values: new[] {
                    new AttributeValue(new PositionTagged<string>("    ", 142), new PositionTagged<object>("foo", 124), literal: true)
                },
                expected: " alt=\"foo\"");
        }

        [Fact]
        public void WriteAttributeToWritesLocalPrefixForSecondValueProvided()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                values: new[] {
                    new AttributeValue(new PositionTagged<string>("    ", 142), new PositionTagged<object>("foo", 124), literal: true),
                    new AttributeValue(new PositionTagged<string>("glorb", 142), new PositionTagged<object>("bar", 124), literal: true)
                },
                expected: " alt=\"fooglorbbar\"");
        }

        [Fact]
        public void WriteAttributeToWritesGlobalPrefixOnlyIfSecondValueIsFirstNonNullOrFalse()
        {
            WriteAttributeTest(
                name: "alt",
                prefix: new PositionTagged<string>(" alt=\"", 42),
                suffix: new PositionTagged<string>("\"", 24),
                values: new[] {
                    new AttributeValue(new PositionTagged<string>("    ", 142), new PositionTagged<object>(null, 124), literal: true),
                    new AttributeValue(new PositionTagged<string>("glorb", 142), new PositionTagged<object>("bar", 124), literal: true)
                },
                expected: " alt=\"bar\"");
        }

        private void WriteAttributeTest(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, string expected)
        {
            WriteAttributeTest(name, prefix, suffix, new AttributeValue[0], expected);
        }

        private void WriteAttributeTest(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, AttributeValue[] values, string expected)
        {
            // Arrange
            var pageMock = new Mock<WebPageExecutingBase>() { CallBase = true };
            pageMock.Setup(p => p.Context).Returns(new Mock<HttpContextBase>().Object);
            pageMock.Object.InstrumentationService.IsAvailable = false;

            StringBuilder written = new StringBuilder();
            StringWriter writer = new StringWriter(written);

            // Act
            pageMock.Object.WriteAttributeTo(writer, name, prefix, suffix, values);

            // Assert
            Assert.Equal(expected, written.ToString());
        }
    }
}
