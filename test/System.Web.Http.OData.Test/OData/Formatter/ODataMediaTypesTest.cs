// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypesTest
    {
        [Fact]
        public void ApplicationAtomXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationAtomXml, ODataMediaTypes.ApplicationAtomXml);
        }

        [Fact]
        public void ApplicationJsonODataVerbose_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataVerbose, ODataMediaTypes.ApplicationJsonODataVerbose);
        }

        [Fact]
        public void ApplicationXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationXml, ODataMediaTypes.ApplicationXml);
        }
    }
}
