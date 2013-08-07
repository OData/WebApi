// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataWorkspaceSerializerTest
    {
        private Type _workspaceType = typeof(ODataWorkspace);

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataWorkspaceSerializer serializer = new ODataWorkspaceSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(42, _workspaceType, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_Graph()
        {
            ODataWorkspaceSerializer serializer = new ODataWorkspaceSerializer();
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(null, type: _workspaceType, messageWriter: null, writeContext: null),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_Throws_CannotWriteType()
        {
            ODataWorkspaceSerializer serializer = new ODataWorkspaceSerializer();
            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(42, _workspaceType, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "ODataWorkspaceSerializer cannot write an object of type 'ODataWorkspace'.");
        }

        [Fact]
        public void ODataWorkspaceSerializer_Works()
        {
            // Arrange
            ODataWorkspaceSerializer serializer = new ODataWorkspaceSerializer();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            // Act
            serializer.WriteObject(new ODataWorkspace(), _workspaceType, new ODataMessageWriter(message), new ODataSerializerContext());

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            XElement element = XElement.Load(stream);
            Assert.Equal("service", element.Name.LocalName);
        }
    }
}
