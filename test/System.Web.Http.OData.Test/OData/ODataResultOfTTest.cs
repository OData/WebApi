// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Formatting;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class ODataResultOfTTest
    {
        [Fact]
        public void ODataResult_SerializesToJson()
        {
            ODataResult<string> result = new ODataResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(ODataResult<string>), result, ms, content: null, transportContext: null).Wait();

            ms.Position = 0;
            Assert.Equal(
                @"{""Items"":[""a"",""b"",""c""],""NextPageLink"":""http://localhost/NextPage"",""Count"":3}",
                new StreamReader(ms).ReadToEnd());
        }

        [Fact]
        public void ODataResult_SerializesToXml()
        {
            ODataResult<string> result = new ODataResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            new XmlMediaTypeFormatter().WriteToStreamAsync(typeof(ODataResult<string>), result, ms, content: null, transportContext: null).Wait();

            ms.Position = 0;
            Assert.Equal(
                @"<ODataResultOfstring xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/System.Web.Http.OData""><Count>3</Count><NextPageLink>http://localhost/NextPage</NextPageLink><Items xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:string>a</d2p1:string><d2p1:string>b</d2p1:string><d2p1:string>c</d2p1:string></Items></ODataResultOfstring>",
                new StreamReader(ms).ReadToEnd());
        }
    }
}
