// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    internal class XmlMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<XmlMediaTypeFormatter, XmlMediaTypeFormatterTracer>
    {
        public override XmlMediaTypeFormatterTracer CreateTracer(XmlMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new XmlMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

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
    }
}
