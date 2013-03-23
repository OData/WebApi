// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Tracing.Tracers
{
    public class MediaTypeFormatterTracerTest
    {
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

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void Inner_Property_On_All_MediaTypeFormatterTracers_Returns_Object_Of_Type_MediaTypeFormatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatterTracer = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);
            
            // Act
            MediaTypeFormatter innerFormatter = (formatterTracer as IDecorator<MediaTypeFormatter>).Inner;      

            // Assert
            Assert.Same(formatter, innerFormatter);
        }

        [Theory]
        [PropertyData("AllKnownFormatters")]
        public void Decorator_GetInner_On_All_MediaTypeFormatterTracers_Returns_Object_Of_Type_MediaTypeFormatter(MediaTypeFormatter formatter)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter formatterTracer = MediaTypeFormatterTracer.CreateTracer(formatter, new TestTraceWriter(), request);

            // Act
            MediaTypeFormatter innerFormatter = Decorator.GetInner(formatterTracer);

            // Assert
            Assert.Same(formatter, innerFormatter);
        }
    }
}
