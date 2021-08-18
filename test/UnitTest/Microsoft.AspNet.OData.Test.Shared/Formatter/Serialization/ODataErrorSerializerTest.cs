//-----------------------------------------------------------------------------
// <copyright file="ODataErrorSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.IO;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Moq;
using Xunit;
#else
using System.IO;
using System.Runtime.Serialization;
using System.Web.Http;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataErrorSerializerTest
    {
        [Fact]
        public void WriteObject_SupportsHttpError()
        {
            var serializer = new ODataErrorSerializer();
#if NETCORE
            var error = new SerializableError();
#else
            var error = new HttpError("bad stuff");
#endif

            Mock<IODataResponseMessage> mockResponseMessage = new Mock<IODataResponseMessage>();
            mockResponseMessage.Setup(response => response.GetStream()).Returns(new MemoryStream());

            ExceptionAssert.DoesNotThrow(() => serializer.WriteObject(error, typeof(ODataError), new ODataMessageWriter(mockResponseMessage.Object), 
                new ODataSerializerContext()));
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_Graph()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();

            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: typeof(ODataError),messageWriter: null, writeContext: null),
                "graph");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();

            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: 42, type: typeof(ODataError), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_ErrorTypeMustBeODataErrorOrHttpError()
        {
            ODataErrorSerializer serializer = new ODataErrorSerializer();
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObject(42, typeof(ODataError), ODataTestUtil.GetMockODataMessageWriter(), new ODataSerializerContext()),
                "The type 'System.Int32' is not supported by the ODataErrorSerializer. The type must be ODataError or HttpError.");
        }

        [Fact]
        public void ODataErrorSerializer_Works()
        {
            // Arrange
            ODataErrorSerializer serializer = new ODataErrorSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataError error = new ODataError { Message = "Error!!!" };
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            settings.SetContentType(ODataFormat.Json);
            ODataMessageWriter writer = new ODataMessageWriter(message, settings);

            // Act
            serializer.WriteObject(error, typeof(ODataError), writer, new ODataSerializerContext());
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"error\":{\"code\":\"\",\"message\":\"Error!!!\"}}", result);
        }
    }
}
