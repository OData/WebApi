// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Formatting.Internal;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Internal
{
    class HttpValueCollectionTest
    {
        public static TheoryDataSet<IEnumerable<KeyValuePair<string, string>>> KeyValuePairs
        {
            get
            {
                return new TheoryDataSet<IEnumerable<KeyValuePair<string, string>>>()
                {
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>(null, null),
                        new KeyValuePair<string,string>("n0", ""),
                        new KeyValuePair<string,string>("n1", "v1"),
                        new KeyValuePair<string,string>("n@2", "v@2"),
                        new KeyValuePair<string,string>("n 3", "v 3"),
                        new KeyValuePair<string,string>("n+4", "v+4"),
                        new KeyValuePair<string,string>("n;5", "v;5"),
                    }
                };
            }
        }

        public static TheoryDataSet<NameValueCollection, string> ToStringTestData
        {
            get
            {
                TheoryDataSet<NameValueCollection, string> dataSet = new TheoryDataSet<NameValueCollection, string>();

                NameValueCollection hvc1 = HttpValueCollection.Create();
                hvc1.Add(null, null);
                dataSet.Add(hvc1, "");

                NameValueCollection hvc2 = HttpValueCollection.Create();
                hvc2.Add("name", null);
                dataSet.Add(hvc2, "name");

                NameValueCollection hvc3 = HttpValueCollection.Create();
                hvc3.Add("name", "");
                dataSet.Add(hvc3, "name");

                NameValueCollection hvc4 = HttpValueCollection.Create();
                hvc4.Add("na me", "");
                dataSet.Add(hvc4, "na+me");

                NameValueCollection hvc5 = HttpValueCollection.Create();
                hvc5.Add("n\",;\\n", "");
                dataSet.Add(hvc5, "n%22%2c%3b%5cn");

                NameValueCollection hvc6 = HttpValueCollection.Create();
                hvc6.Add("", "v1");
                hvc6.Add("", "v2");
                hvc6.Add("", "v3");
                hvc6.Add("", "v4");
                dataSet.Add(hvc6, "=v1&=v2&=v3&=v4");

                NameValueCollection hvc7 = HttpValueCollection.Create();
                hvc7.Add("n1", "v1");
                hvc7.Add("n2", "v2");
                hvc7.Add("n3", "v3");
                hvc7.Add("n4", "v4");
                dataSet.Add(hvc7, "n1=v1&n2=v2&n3=v3&n4=v4");

                NameValueCollection hvc8 = HttpValueCollection.Create();
                hvc8.Add("n,1", "v,1");
                hvc8.Add("n;2", "v;2");
                dataSet.Add(hvc8, "n%2c1=v%2c1&n%3b2=v%3b2");

                NameValueCollection hvc9 = HttpValueCollection.Create();
                hvc9.Add("n1", "&");
                hvc9.Add("n2", ";");
                dataSet.Add(hvc9, "n1=%26&n2=%3b");

                NameValueCollection hvc10 = HttpValueCollection.Create();
                hvc10.Add("n1", "&");
                hvc10.Add("n2", null);
                hvc10.Add("n3", "null");
                dataSet.Add(hvc10, "n1=%26&n2&n3=null");

                return dataSet;
            }
        }

        [Fact]
        public void Create_CreatesEmptyCollection()
        {
            NameValueCollection nvc = HttpValueCollection.Create();

            Assert.IsType<HttpValueCollection>(nvc);
            Assert.Equal(0, nvc.Count);
        }

        [Theory]
        [PropertyData("KeyValuePairs")]
        public void Create_InitializesCorrectly(IEnumerable<KeyValuePair<string, string>> input)
        {
            NameValueCollection nvc = HttpValueCollection.Create(input);

            int count = input.Count();
            Assert.IsType<HttpValueCollection>(nvc);
            Assert.Equal(count, nvc.Count);

            int index = 0;
            foreach (KeyValuePair<string, string> kvp in input)
            {
                string expectedKey = kvp.Key ?? String.Empty;
                string expectedValue = kvp.Value ?? String.Empty;

                string actualKey = nvc.AllKeys[index];
                string actualValue = nvc[index];
                index++;

                Assert.Equal(expectedKey, actualKey);
                Assert.Equal(expectedValue, actualValue);
            }
        }

        [Theory]
        [PropertyData("ToStringTestData")]
        public void ToString_GeneratesCorrectOutput(NameValueCollection input, string expectedOutput)
        {
            string actualOutput = input.ToString();
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
