// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class PerRequestParameterBindingTest
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Formatters()
        {
            Assert.ThrowsArgumentNull(
                () => new PerRequestParameterBinding(descriptor: new Mock<HttpParameterDescriptor>().Object, formatters: null),
                "formatters");
        }

        [Fact]
        public void WillReadBody_ReturnsTrue()
        {
            PerRequestParameterBinding binding = new PerRequestParameterBinding(new Mock<HttpParameterDescriptor>().Object, Enumerable.Empty<MediaTypeFormatter>());

            Assert.True(binding.WillReadBody);
        }

        [Fact]
        public void ExecuteBindingAsync_CallsGetPerRequestFormatterInstance_BeforeBinding()
        {
            // Arrange
            Mock<HttpParameterDescriptor> parameter = new Mock<HttpParameterDescriptor>();
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            MediaTypeFormatter perRequestFormatter = new Mock<MediaTypeFormatter>().Object;

            MockPerRequestParameterBinding binding = new MockPerRequestParameterBinding(parameter.Object, new[] { formatter.Object });

            HttpRequestMessage request = new HttpRequestMessage();
            request.Content = new StringContent("{}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/something");
            HttpControllerContext controllercontext = new HttpControllerContext { Request = request };
            HttpActionContext actionContext = new HttpActionContext(controllercontext, new Mock<HttpActionDescriptor>().Object);

            MediaTypeFormatter formatterUsedForPrameterBinding = null;
            binding.CreateInnerBindingFunc = (formatters) =>
                {
                    formatterUsedForPrameterBinding = formatters.Single();
                    return new VoidParameterBinding();
                };

            formatter
                .Setup(f => f.GetPerRequestFormatterInstance(parameter.Object.ParameterType, request, request.Content.Headers.ContentType))
                .Returns(perRequestFormatter);

            // Act
            binding.ExecuteBindingAsync(new Mock<ModelMetadataProvider>().Object, actionContext, new CancellationToken());

            // Assert
            Assert.Same(formatterUsedForPrameterBinding, perRequestFormatter);
        }

        private class VoidParameterBinding : HttpParameterBinding
        {
            public VoidParameterBinding()
                : base(new Mock<HttpParameterDescriptor>().Object)
            {
            }

            public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                return Task.FromResult(42);
            }
        }

        private class MockPerRequestParameterBinding : PerRequestParameterBinding
        {
            public MockPerRequestParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters)
                : base(descriptor, formatters)
            {
            }

            public Func<IEnumerable<MediaTypeFormatter>, HttpParameterBinding> CreateInnerBindingFunc { get; set; }

            protected override HttpParameterBinding CreateInnerBinding(IEnumerable<MediaTypeFormatter> perRequestFormatters)
            {
                if (CreateInnerBindingFunc != null)
                {
                    return CreateInnerBindingFunc(perRequestFormatters);
                }

                return null;
            }
        }
    }
}
