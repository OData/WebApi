// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    internal class JsonMediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<JsonMediaTypeFormatter, JsonMediaTypeFormatterTracer>
    {
        public override JsonMediaTypeFormatterTracer CreateTracer(JsonMediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new JsonMediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        [Fact]
        public void UseDataContractJsonSerializer_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.UseDataContractJsonSerializer = !innerFormatter.UseDataContractJsonSerializer;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.UseDataContractJsonSerializer, tracer.UseDataContractJsonSerializer);
        }

        [Fact]
        public void MaxDepth_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.MaxDepth = innerFormatter.MaxDepth + 1;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.MaxDepth, tracer.MaxDepth);
        }

        [Fact]
        public void Indent_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter();
            innerFormatter.Indent = !innerFormatter.Indent;
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Equal(innerFormatter.Indent, tracer.Indent);
        }

        [Fact]
        public void SerializerSettings_Uses_Inners()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            JsonMediaTypeFormatter innerFormatter = new JsonMediaTypeFormatter() { SerializerSettings = serializerSettings };
            JsonMediaTypeFormatterTracer tracer = new JsonMediaTypeFormatterTracer(innerFormatter, new TestTraceWriter(), request);

            // Act & Assert
            Assert.Same(innerFormatter.SerializerSettings, tracer.SerializerSettings);
        }

    }
}
