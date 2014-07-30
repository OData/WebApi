// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Web.Http.OData.Formatter;
using System.Xml.Linq;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Builder
{
    public class ODataUtilsLearningTests
    {
        [Fact]
        public void EntityContainer_Is_Default_ShowsUp_In_Metadata()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityContainer container = new EdmEntityContainer("Default", "SampleContainer");
            model.AddElement(container);

            // Act
            model.SetIsDefaultEntityContainer(container, isDefaultContainer: true);

            // Assert
            MemoryStream stream = new MemoryStream();
            ODataMessageWriter writer = new ODataMessageWriter(new ODataMessageWrapper(stream) as IODataResponseMessage, new ODataMessageWriterSettings(), model);
            writer.WriteMetadataDocument();
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            var containerXml = element.Descendants().Where(n => n.Name.LocalName == "EntityContainer").SingleOrDefault();
            Assert.Equal("SampleContainer", containerXml.Attribute("Name").Value);
            Assert.Equal("true", containerXml.Attributes().Where(a => a.Name.LocalName == "IsDefaultEntityContainer").Single().Value);
        }
    }
}
