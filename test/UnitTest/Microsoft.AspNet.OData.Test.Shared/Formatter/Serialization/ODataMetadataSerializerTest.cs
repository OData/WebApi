//-----------------------------------------------------------------------------
// <copyright file="ODataMetadataSerializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

#if NETCOREAPP3_1
        [Fact]
        public async Task ODataMetadataSerializer_Works_Async()
        {
            // Arrange
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();
            IEdmModel model = new EdmModel();

            // 1) XML
            // Act
            string payload = await this.WriteAndGetPayloadAsync(model, "application/xml", async omWriter =>
            {
                await serializer.WriteObjectAsync("42" /*useless*/, typeof(IEdmModel), omWriter, new ODataSerializerContext());
            });

            // Assert
            Assert.Contains("<edmx:Edmx Version=\"4.0\"", payload);

            // 2) JSON
            // Act
            payload = await this.WriteAndGetPayloadAsync(model, "application/json", async omWriter =>
            {
                await serializer.WriteObjectAsync("42" /*useless*/, typeof(IEdmModel), omWriter, new ODataSerializerContext());
            });

            // Assert
            Assert.Equal(@"{
  ""$Version"": ""4.0""
}", payload);
        }

        [Fact]
        public async Task ODataMetadataSerializer_Works_ForSingleton_Async()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Singleton<Customer>("Me");
            IEdmModel model = builder.GetEdmModel();
            ODataMetadataSerializer serializer = new ODataMetadataSerializer();

            // XML
            // Act
            string payload = await this.WriteAndGetPayloadAsync(model, "application/xml", async omWriter =>
            {
                await serializer.WriteObjectAsync(model, typeof(IEdmModel), omWriter, new ODataSerializerContext());
            });

            // Assert
            Assert.Contains("<Singleton Name=\"Me\" Type=\"Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.Customer\" />", payload);

            // JSON
            // Act
            payload = await this.WriteAndGetPayloadAsync(model, "application/json", async omWriter =>
            {
                await serializer.WriteObjectAsync(model, typeof(IEdmModel), omWriter, new ODataSerializerContext());
            });

            // Assert
            Assert.Contains(@"  ""Default"": {
    ""Container"": {
      ""$Kind"": ""EntityContainer"",
      ""Me"": {
        ""$Type"": ""Microsoft.AspNet.OData.Test.Formatter.Serialization.Models.Customer""
      }
    }
  }", payload);
        }

        private async Task<string> WriteAndGetPayloadAsync(IEdmModel edmModel, string contentType, Func<ODataMessageWriter, Task> test)
        {
            MemoryStream stream = new MemoryStream();
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                // the content type is necessary to write the metadata in async?
                { "Content-Type", contentType}
            };

            IODataResponseMessage message = new ODataMessageWrapper(stream, headers);

            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();
            writerSettings.EnableMessageStreamDisposal = false;
            writerSettings.BaseUri = new Uri("http://www.example.com/");

            using (var msgWriter = new ODataMessageWriter((IODataResponseMessageAsync)message, writerSettings, edmModel))
            {
                await test(msgWriter);
            }

            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
#endif
    }
}
