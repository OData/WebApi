// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Formatting;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class PageResultOfTTest
    {
        [Fact]
        public void PageResult_SerializesToJson()
        {
            PageResult<string> result = new PageResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(PageResult<string>), result, ms, content: null, transportContext: null).Wait();

            ms.Position = 0;
            Assert.Equal(
                @"{""Items"":[""a"",""b"",""c""],""NextPageLink"":""http://localhost/NextPage"",""Count"":3}",
                new StreamReader(ms).ReadToEnd());
        }

        [Fact]
        public void PageResult_SerializesToXml()
        {
            PageResult<string> result = new PageResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            new XmlMediaTypeFormatter().WriteToStreamAsync(typeof(PageResult<string>), result, ms, content: null, transportContext: null).Wait();

            ms.Position = 0;
            Assert.Equal(
                @"<PageResultOfstring xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/System.Web.Http.OData""><Count>3</Count><NextPageLink>http://localhost/NextPage</NextPageLink><Items xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:string>a</d2p1:string><d2p1:string>b</d2p1:string><d2p1:string>c</d2p1:string></Items></PageResultOfstring>",
                new StreamReader(ms).ReadToEnd());
        }

        [Fact]
        public void EmptyPageResult_CanBeCreated()
        {
            Assert.DoesNotThrow(() => new PageResult<string>(new string[] {}, null, 0));
        }

        [Fact]
        public void Ctor_Throws_OnNegativeCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PageResult<string>(new string[] { }, null, -1),
                "Value must be greater than or equal to 0.\r\nParameter name: value\r\nActual value was -1.");
        }
    }
}
