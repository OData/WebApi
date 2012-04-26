// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    internal class FormUrlEncodedMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<FormUrlEncodedMediaTypeFormatter, FormUrlEncodedMediaTypeFormatterTracer>
    {
        public override FormUrlEncodedMediaTypeFormatterTracer CreateTracer(FormUrlEncodedMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new FormUrlEncodedMediaTypeFormatterTracer(formatter, traceWriter, request);
        }
        
        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            FormUrlEncodedMediaTypeFormatter innerFormatter = new FormUrlEncodedMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            FormUrlEncodedMediaTypeFormatterTracer tracer = new FormUrlEncodedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void ReadBufferSize_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            FormUrlEncodedMediaTypeFormatter innerFormatter = new FormUrlEncodedMediaTypeFormatter();
            innerFormatter.ReadBufferSize = innerFormatter.ReadBufferSize + 1;
            FormUrlEncodedMediaTypeFormatterTracer tracer = new FormUrlEncodedMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.ReadBufferSize, tracer.ReadBufferSize);
        }
    }
}
