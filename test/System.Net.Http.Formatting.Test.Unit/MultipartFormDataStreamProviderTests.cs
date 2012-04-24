// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class MockMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public MockMultipartFormDataStreamProvider()
            : base(Path.GetTempPath())
        {
        }

        public MockMultipartFormDataStreamProvider(string rootPath)
            : base(rootPath)
        {
        }

        public MockMultipartFormDataStreamProvider(string rootPath, int bufferSize)
            : base(rootPath, bufferSize)
        {
        }
    }

    public class MultipartFormDataStreamProviderTests : MultipartStreamProviderTestBase<MockMultipartFormDataStreamProvider>
    {
        private const int ValidBufferSize = 0x111;
        private const string ValidPath = @"c:\some\path";

        [Fact]
        public void FormData_IsEmpty()
        {
            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath, ValidBufferSize);
            Assert.Empty(provider.FormData);
        }

        [Fact]
        public void GetStream_ThrowsOnNoContentDisposition()
        {
            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();

            Assert.Throws<InvalidOperationException>(() => { provider.GetStream(content, headers); });
        }

        [Fact]
        public void GetStream()
        {
            Stream stream0 = null;
            Stream stream1 = null;

            try
            {
                string tempPath = Path.GetTempPath();
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StringContent("Content 1"), "NoFile");
                content.Add(new StringContent("Content 2"), "File", "Filename");

                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(tempPath);
                stream0 = provider.GetStream(content, content.ElementAt(0).Headers);
                stream1 = provider.GetStream(content, content.ElementAt(1).Headers);

                Assert.IsType<MemoryStream>(stream0);
                Assert.IsType<FileStream>(stream1);

                Assert.Equal(1, provider.FileData.Count);
                string partialFileName = String.Format("{0}BodyPart_", tempPath);
                Assert.Contains(partialFileName, provider.FileData[0].LocalFileName);

                Assert.Same(content.ElementAt(1).Headers.ContentDisposition, provider.FileData[0].Headers.ContentDisposition);
            }
            finally
            {
                if (stream0 != null)
                {
                    stream0.Close();
                }

                if (stream1 != null)
                {
                    stream1.Close();
                }
            }
        }

        [Fact]
        public Task PostProcessing_ProcessesFormData()
        {
            // Arrange
            int maxContents = 16;
            string contentFormat = "Content {0}";
            string formNameFormat = "FormName_{0}";

            MultipartFormDataContent multipartContent = new MultipartFormDataContent();

            for (int index = 0; index < maxContents; index++)
            {
                string content = String.Format(contentFormat, index);
                string formName = String.Format(formNameFormat, index);
                multipartContent.Add(new StringContent(content), formName);
            }

            MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(ValidPath);
            foreach (HttpContent content in multipartContent)
            {
                provider.Contents.Add(content);
                provider.GetStream(multipartContent, content.Headers);
            }

            // Act
            return provider.ExecutePostProcessingAsync().ContinueWith(
                processingTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, processingTask.Status);
                    Assert.Equal(maxContents, provider.FormData.Count);

                    for (int index = 0; index < maxContents; index++)
                    {
                        string content = String.Format(contentFormat, index);
                        string formName = String.Format(formNameFormat, index);
                        Assert.Equal(content, provider.FormData[formName]);
                    }
                });
        }
    }
}
