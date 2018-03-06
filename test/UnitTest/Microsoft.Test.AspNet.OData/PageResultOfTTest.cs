// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.Test.AspNet.OData.Common;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class PageResultOfTTest
    {
        [Fact]
        public async Task PageResult_SerializesToJson()
        {
            PageResult<string> result = new PageResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            await new JsonMediaTypeFormatter().WriteToStreamAsync(typeof(PageResult<string>), result, ms, content: null, transportContext: null);

            ms.Position = 0;
            Assert.Equal(
                @"{""Items"":[""a"",""b"",""c""],""NextPageLink"":""http://localhost/NextPage"",""Count"":3}",
                new StreamReader(ms).ReadToEnd());
        }

        [Fact]
        public async Task PageResult_SerializesToXml()
        {
            PageResult<string> result = new PageResult<string>(new string[] { "a", "b", "c" }, new Uri("http://localhost/NextPage"), 3);
            MemoryStream ms = new MemoryStream();

            await new XmlMediaTypeFormatter().WriteToStreamAsync(typeof(PageResult<string>), result, ms, content: null, transportContext: null);

            ms.Position = 0;
            Assert.Equal(
                @"<PageResultOfstring xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/Microsoft.AspNet.OData""><Count>3</Count><NextPageLink>http://localhost/NextPage</NextPageLink><Items xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:string>a</d2p1:string><d2p1:string>b</d2p1:string><d2p1:string>c</d2p1:string></Items></PageResultOfstring>",
                new StreamReader(ms).ReadToEnd());
        }

        [Fact]
        public void EmptyPageResult_CanBeCreated()
        {
            ExceptionAssert.DoesNotThrow(() => new PageResult<string>(new string[] {}, null, 0));
        }

        [Fact]
        public void Ctor_Throws_OnNegativeCount()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => new PageResult<string>(new string[] { }, null, -1),
                "Value must be greater than or equal to 0.\r\nParameter name: value\r\nActual value was -1.");
        }
    }
}
#endif