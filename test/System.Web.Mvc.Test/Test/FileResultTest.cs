// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Mime;
using System.Text;
using System.Web.TestUtil;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class FileResultTest
    {
        [Fact]
        public void ConstructorSetsContentTypeProperty()
        {
            // Act
            FileResult result = new EmptyFileResult("someContentType");

            // Assert
            Assert.Equal("someContentType", result.ContentType);
        }

        [Fact]
        public void ConstructorThrowsIfContentTypeIsEmpty()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new EmptyFileResult(String.Empty); }, "contentType");
        }

        [Fact]
        public void ConstructorThrowsIfContentTypeIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNullOrEmpty(
                delegate { new EmptyFileResult(null); }, "contentType");
        }

        [Fact]
        public void ContentDispositionHeaderIsEncodedCorrectly()
        {
            // See comment in FileResult.cs detailing how the FileDownloadName should be encoded.

            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/my-type").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.AddHeader("Content-Disposition", @"attachment; filename=""some\\file""")).Verifiable();

            EmptyFileResult result = new EmptyFileResult("application/my-type")
            {
                FileDownloadName = @"some\file"
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            Assert.True(result.WasWriteFileCalled);
            mockControllerContext.Verify();
        }

        [Fact]
        public void ContentDispositionHeaderIsEncodedCorrectlyForUnicodeCharacters()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/my-type").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.AddHeader("Content-Disposition", @"attachment; filename*=UTF-8''ABCXYZabcxyz012789!%40%23$%25%5E&%2A%28%29-%3D_+.:~%CE%94")).Verifiable();

            EmptyFileResult result = new EmptyFileResult("application/my-type")
            {
                FileDownloadName = "ABCXYZabcxyz012789!@#$%^&*()-=_+.:~Δ"
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            Assert.True(result.WasWriteFileCalled);
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultDoesNotSetContentDispositionIfNotSpecified()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/my-type").Verifiable();

            EmptyFileResult result = new EmptyFileResult("application/my-type");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            Assert.True(result.WasWriteFileCalled);
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultSetsContentDispositionIfSpecified()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>(MockBehavior.Strict);
            mockControllerContext.SetupSet(c => c.HttpContext.Response.ContentType = "application/my-type").Verifiable();
            mockControllerContext.Setup(c => c.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=filename.ext")).Verifiable();

            EmptyFileResult result = new EmptyFileResult("application/my-type")
            {
                FileDownloadName = "filename.ext"
            };

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            Assert.True(result.WasWriteFileCalled);
            mockControllerContext.Verify();
        }

        [Fact]
        public void ExecuteResultThrowsIfContextIsNull()
        {
            // Arrange
            FileResult result = new EmptyFileResult();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { result.ExecuteResult(null); }, "context");
        }

        [Fact]
        public void FileDownloadNameProperty()
        {
            // Arrange
            FileResult result = new EmptyFileResult();

            // Act & assert
            MemberHelper.TestStringProperty(result, "FileDownloadName", String.Empty);
        }

        [Theory]
        [InlineData("09aAzZ", "attachment; filename=09aAzZ")]
        [InlineData(" ", "attachment; filename=\" \"")]
        [InlineData("a b", "attachment; filename=\"a b\"")]
        [InlineData("a\tb", "attachment; filename=\"a\tb\"")]
        [InlineData("a\nb", "attachment; filename=\"a\\\nb\"")]
        [InlineData("a.b", "attachment; filename=a.b")]
        [InlineData("-", "attachment; filename=-")]
        [InlineData("_", "attachment; filename=_")]
        [InlineData(":", "attachment; filename=\":\"")]
        [InlineData(": :", "attachment; filename=\": :\"")]
        [InlineData("~", "attachment; filename=~")]
        [InlineData("$", "attachment; filename=$")]
        [InlineData("&", "attachment; filename=&")]
        [InlineData("+", "attachment; filename=+")]
        [InlineData("@", "attachment; filename=\"@\"")]
        [InlineData("\"", "attachment; filename=\"\\\"\"")]
        [InlineData("#", "attachment; filename=#")]
        [InlineData("résumé.txt", "attachment; filename*=UTF-8''r%C3%A9sum%C3%A9.txt")]
        [InlineData("Δ", "attachment; filename*=UTF-8''%CE%94")]
        [InlineData("Δ\t", "attachment; filename*=UTF-8''%CE%94%09")]
        [InlineData("ABCXYZabcxyz012789!@#$%^&*()-=_+.:~Δ", @"attachment; filename*=UTF-8''ABCXYZabcxyz012789!%40%23$%25%5E&%2A%28%29-%3D_+.:~%CE%94")]
        [CLSCompliant(false)]
        public void GetHeaderValue_Produces_Correct_ContentDisposition(string input, string expectedOutput)
        {
            // Arrange & Act
            string actual = FileResult.ContentDispositionUtil.GetHeaderValue(input);

            // Assert
            Assert.Equal(expectedOutput, actual);
        }

        private class EmptyFileResult : FileResult
        {
            public bool WasWriteFileCalled;

            public EmptyFileResult()
                : this(MediaTypeNames.Application.Octet)
            {
            }

            public EmptyFileResult(string contentType)
                : base(contentType)
            {
            }

            protected override void WriteFile(HttpResponseBase response)
            {
                WasWriteFileCalled = true;
            }
        }
    }
}
