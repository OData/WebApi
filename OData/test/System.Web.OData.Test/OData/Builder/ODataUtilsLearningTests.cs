// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Web.OData.Formatter;
using System.Xml.Linq;
using Microsoft.OData.Core;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ODataUtilsLearningTests
    {
        [Fact]
        public void EntityContainer_Is_Default_DoesNotShowUp_In_Metadata()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("Default", "SampleContainer");
            model.AddElement(container);

            // Act & Assert
            MemoryStream stream = new MemoryStream();
            ODataMessageWriter writer = new ODataMessageWriter(new ODataMessageWrapper(stream) as IODataResponseMessage, new ODataMessageWriterSettings(), model);
            writer.WriteMetadataDocument();
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            var containerXml = element.Descendants().SingleOrDefault(n => n.Name.LocalName == "EntityContainer");
            Assert.NotNull(containerXml);
            Assert.Equal("SampleContainer", containerXml.Attribute("Name").Value);
            Assert.Null(containerXml.Attributes().FirstOrDefault(a => a.Name.LocalName == "IsDefaultEntityContainer"));
        }
    }
}
