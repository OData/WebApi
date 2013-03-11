// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ModelBinding
{
    public class FormatterParameterBindingTest
    {
        [Fact]
        public void ReadContentAsync_Throws_ForNoContentType()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Content = new StringContent("The quick, brown fox tripped and fell.");
            request.Content.Headers.ContentType = null;
            var formatters = new MediaTypeFormatterCollection();
            var descriptor = new Mock<HttpParameterDescriptor>();
            descriptor.Setup(desc => desc.IsOptional).Returns(false);
            var binding = new FormatterParameterBinding(descriptor.Object, formatters, null);

            HttpResponseException exception = Assert.Throws<HttpResponseException>(
                () => binding.ReadContentAsync(request, typeof(string), formatters, null));

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, exception.Response.StatusCode);
            HttpError error;
            exception.Response.TryGetContentValue(out error);
            Assert.Equal(
                "The request contains an entity body but no Content-Type header. The inferred media type 'application/octet-stream' is not supported for this resource.",
                error.Message);
        }

        [Fact]
        public void ReadContentAsync_Throws_ForUnsupportedMediaType()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost");
            request.Content = new StringContent("The quick, brown fox tripped and fell.");
            var formatters = new MediaTypeFormatterCollection();
            var descriptor = new Mock<HttpParameterDescriptor>();
            descriptor.Setup(desc => desc.IsOptional).Returns(false);
            var binding = new FormatterParameterBinding(descriptor.Object, formatters, null);

            HttpResponseException exception = Assert.Throws<HttpResponseException>(
                () => binding.ReadContentAsync(request, typeof(string), formatters, null));

            Assert.Equal(HttpStatusCode.UnsupportedMediaType, exception.Response.StatusCode);
            HttpError error;
            exception.Response.TryGetContentValue(out error);
            Assert.Equal(
                "The request entity's media type 'text/plain' is not supported for this resource.",
                error.Message);
        }    
    }
}
