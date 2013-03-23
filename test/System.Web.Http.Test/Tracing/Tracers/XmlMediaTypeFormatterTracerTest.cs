// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Services;
using Microsoft.TestCommon;

namespace System.Web.Http.Tracing.Tracers
{
    public class XmlMediaTypeFormatterTracerTest
    {
        [Fact]
        public void UseXmlSerializer_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.UseXmlSerializer = !innerFormatter.UseXmlSerializer;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.UseXmlSerializer, tracer.UseXmlSerializer);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void Indent_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            XmlMediaTypeFormatter innerFormatter = new XmlMediaTypeFormatter();
            innerFormatter.Indent = !innerFormatter.Indent;
            XmlMediaTypeFormatterTracer tracer = new XmlMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.Indent, tracer.Indent);
        }

        [Fact]
        public void Inner_Property_On_XmlMediaTypeFormatterTracer_Returns_XmlMediaTypeFormatter()
        {
            // Arrange
            XmlMediaTypeFormatter expectedInner = new XmlMediaTypeFormatter();
            XmlMediaTypeFormatterTracer productUnderTest = new XmlMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            XmlMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_XmlMediaTypeFormatterTracer_Returns_XmlMediaTypeFormatter()
        {
            // Arrange
            XmlMediaTypeFormatter expectedInner = new XmlMediaTypeFormatter();
            XmlMediaTypeFormatterTracer productUnderTest = new XmlMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            XmlMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as XmlMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}
