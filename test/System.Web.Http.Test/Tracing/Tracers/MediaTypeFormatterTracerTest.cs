// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using Microsoft.TestCommon;
using Moq;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    internal class MediaTypeFormatterTracerTest : ReadWriteMediaTypeFormatterTracerTestBase<MediaTypeFormatter, MediaTypeFormatterTracer>
    {
        public override MediaTypeFormatterTracer CreateTracer(MediaTypeFormatter formatter, HttpRequestMessage request, ITraceWriter traceWriter)
        {
            return new MediaTypeFormatterTracer(formatter, traceWriter, request);
        }

        public static TheoryDataSet<MediaTypeFormatter> AllKnownFormatters
        {
            get
            {
                return new TheoryDataSet<MediaTypeFormatter>
                {
                    new XmlMediaTypeFormatter(), 
                    new JsonMediaTypeFormatter(),
                    new FormUrlEncodedMediaTypeFormatter(),
                    new Mock<BufferedMediaTypeFormatter>().Object
                };
            }
        }

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void CreateTracer_Returns_Tracing_Formatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            MediaTypeFormatter tracingFormatter = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Assert
            IFormatterTracer tracer = Assert.IsAssignableFrom<IFormatterTracer>(tracingFormatter);
            Assert.Same(formatter, tracer.InnerFormatter);
        }
    }
}
