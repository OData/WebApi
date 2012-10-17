// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataErrorSerializerTest
    {
        [Fact]
        public void WriteObject_SupportsHttpError()
        {
            var serializer = new ODataErrorSerializer();
            var error = new HttpError("bad stuff");
            Mock<IODataResponseMessage> mockResponseMessage = new Mock<IODataResponseMessage>();
            mockResponseMessage.Setup(response => response.GetStream()).Returns(new MemoryStream());

            Assert.DoesNotThrow(() => serializer.WriteObject(error, new ODataMessageWriter(mockResponseMessage.Object), new ODataSerializerContext()));
        }
    }
}
