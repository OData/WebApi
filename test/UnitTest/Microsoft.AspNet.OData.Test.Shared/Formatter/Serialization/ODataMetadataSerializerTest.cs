// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataMetadataSerializerTest
    {
        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(42, typeof(IEdmModel), messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void ODataMetadataSerializer_Works()
        {
            // Arrange
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings();
            IEdmModel model = new EdmModel();

            // Act
            serializer.WriteObject("42", typeof(IEdmModel), new ODataMessageWriter(message, settings, model), new ODataSerializerContext());

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("Edmx", element.Name.LocalName);
        }

        [Fact]
        public void ODataMetadataSerializer_Works_ForSingleton()
        {
            // Arrange
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.Singleton<Customer>("Me");
            builder.EntitySet<Order>("MyOrders");
            IEdmModel model = builder.GetEdmModel();

            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings();

            // Act
            serializer.WriteObject(model, typeof(IEdmModel), new ODataMessageWriter(message, settings, model), new ODataSerializerContext());

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Contains("<Singleton Name=\"Me\" Type=\"Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.Customer\">", result);
        }
    }
}
