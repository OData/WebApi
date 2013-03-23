// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Newtonsoft.Json;

namespace System.Web.Http.Tracing.Tracers
{
    public class JsonMediaTypeFormatterTracerTest 
    {
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

        [Fact]
        public void Inner_Property_On_JsonMediaTypeFormatterTracerTest_Returns_JsonMediaTypeFormatter()
        {
            // Arrange
            JsonMediaTypeFormatter expectedInner = new JsonMediaTypeFormatter();
            JsonMediaTypeFormatterTracer productUnderTest = new JsonMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            JsonMediaTypeFormatter actualInner = productUnderTest.Inner;

            // Assert
            Assert.Same(expectedInner, actualInner);
        }

        [Fact]
        public void Decorator_GetInner_On_JsonMediaTypeFormatterTracerTest_Returns_JsonMediaTypeFormatter()
        {
            // Arrange
            JsonMediaTypeFormatter expectedInner = new JsonMediaTypeFormatter();
            JsonMediaTypeFormatterTracer productUnderTest = new JsonMediaTypeFormatterTracer(expectedInner, new TestTraceWriter(), new HttpRequestMessage());

            // Act
            JsonMediaTypeFormatter actualInner = Decorator.GetInner(productUnderTest as JsonMediaTypeFormatter);

            // Assert
            Assert.Same(expectedInner, actualInner);
        }
    }
}
