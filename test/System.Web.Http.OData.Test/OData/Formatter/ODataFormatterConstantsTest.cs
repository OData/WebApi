// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter
{
    public class ODataFormatterConstantsTest
    {
        [Fact]
        public void ApplicationAtomXmlMediaType_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataFormatterConstants.ApplicationAtomXmlMediaType, ODataFormatterConstants.ApplicationAtomXmlMediaType);
        }

        [Fact]
        public void ApplicationJsonMediaType_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataFormatterConstants.ApplicationJsonMediaType, ODataFormatterConstants.ApplicationJsonMediaType);
        }

        [Fact]
        public void ApplicationXmlMediaType_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataFormatterConstants.ApplicationXmlMediaType, ODataFormatterConstants.ApplicationXmlMediaType);
        }
    }
}
