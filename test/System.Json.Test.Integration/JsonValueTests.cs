// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace System.Json
{
    /// <summary>
    /// JsonValue unit tests
    /// </summary>
    public class JsonValueTests
    {

        public static IEnumerable<object[]> StreamLoadingTestData
        {
            get
            {
                bool[] useSeekableStreams = new bool[] { true, false };
                Dictionary<string, Encoding> allEncodings = new Dictionary<string, Encoding>
                {
                    { "UTF8, no BOM", new UTF8Encoding(false) },
                    { "Unicode, no BOM", new UnicodeEncoding(false, false) },
                    { "BigEndianUnicode, no BOM", new UnicodeEncoding(true, false) },
                };

                string[] jsonStrings = { "[1, 2, null, false, {\"foo\": 1, \"bar\":true, \"baz\":null}, 1.23e+56]", "4" };

                foreach (string jsonString in jsonStrings)
                {
                    foreach (bool useSeekableStream in useSeekableStreams)
                    {
                        foreach (var kvp in allEncodings)
                        {
                            yield return new object[] { jsonString, useSeekableStream, kvp.Key, kvp.Value };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests for <see cref="JsonValue.Load(Stream)"/>.
        /// </summary>
        [Theory]
        [PropertyData("StreamLoadingTestData")]
        public void StreamLoading(string jsonString, bool useSeekableStream, string encodingName, Encoding encoding)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms, encoding);
                sw.Write(jsonString);
                sw.Flush();
                Log.Info("[{0}] {1}: size of the json stream: {2}", useSeekableStream ? "seekable" : "non-seekable", encodingName, ms.Position);
                ms.Position = 0;
                JsonValue parsed = JsonValue.Parse(jsonString);
                JsonValue loaded = useSeekableStream ? JsonValue.Load(ms) : JsonValue.Load(new NonSeekableStream(ms));
                using (StringReader sr = new StringReader(jsonString))
                {
                    JsonValue loadedFromTextReader = JsonValue.Load(sr);
                    Assert.Equal(parsed.ToString(), loaded.ToString());
                    Assert.Equal(parsed.ToString(), loadedFromTextReader.ToString());
                }
            }
        }

        [Fact]
        public void ZeroedStreamLoadingThrowsFormatException()
        {
            ExpectException<FormatException>(delegate
            {
                using (MemoryStream ms = new MemoryStream(new byte[10]))
                {
                    JsonValue.Load(ms);
                }
            });
        }

        /// <summary>
        /// Tests for handling with escaped characters.
        /// </summary>
        [Fact]
        public void EscapedCharacters()
        {
            string str = null;
            JsonValue value = null;
            str = (string)value;
            Assert.Null(str);
            value = "abc\b\t\r\u1234\uDC80\uDB11def\\\0ghi";
            str = (string)value;
            Assert.Equal("\"abc\\u0008\\u0009\\u000d\u1234\\udc80\\udb11def\\\\\\u0000ghi\"", value.ToString());
            value = '\u0000';
            str = (string)value;
            Assert.Equal("\u0000", str);
        }

        /// <summary>
        /// Tests for JSON objects with the special '__type' object member.
        /// </summary>
        [Fact]
        public void TypeHintAttributeTests()
        {
            string json = "{\"__type\":\"TypeHint\",\"a\":123}";
            JsonValue jv = JsonValue.Parse(json);
            string newJson = jv.ToString();
            Assert.Equal(json, newJson);

            json = "{\"b\":567,\"__type\":\"TypeHint\",\"a\":123}";
            jv = JsonValue.Parse(json);
            newJson = jv.ToString();
            Assert.Equal(json, newJson);

            json = "[12,{\"__type\":\"TypeHint\",\"a\":123,\"obj\":{\"__type\":\"hint2\",\"b\":333}},null]";
            jv = JsonValue.Parse(json);
            newJson = jv.ToString();
            Assert.Equal(json, newJson);
        }

        /// <summary>
        /// Tests for reading JSON with different member names.
        /// </summary>
        [Fact]
        public void ObjectNameTests()
        {
            string[] objectNames = new string[]
            {
                "simple",
                "with spaces",
                "with<>brackets",
                "",
            };

            foreach (string objectName in objectNames)
            {
                string json = String.Format(CultureInfo.InvariantCulture, "{{\"{0}\":123}}", objectName);
                JsonValue jv = JsonValue.Parse(json);
                Assert.Equal(123, jv[objectName].ReadAs<int>());
                string newJson = jv.ToString();
                Assert.Equal(json, newJson);

                JsonObject jo = new JsonObject { { objectName, 123 } };
                Assert.Equal(123, jo[objectName].ReadAs<int>());
                newJson = jo.ToString();
                Assert.Equal(json, newJson);
            }

            ExpectException<FormatException>(() => JsonValue.Parse("{\"nonXmlChar\u0000\":123}"));
        }

        /// <summary>
        /// Miscellaneous tests for parsing JSON.
        /// </summary>
        [Fact]
        public void ParseMiscellaneousTest()
        {
            string[] jsonValues =
            {
                "[]",
                "[1]",
                "[1,2,3,[4.1,4.2],5]",
                "{}",
                "{\"a\":1}",
                "{\"a\":1,\"b\":2,\"c\":3,\"d\":4}",
                "{\"a\":1,\"b\":[2,3],\"c\":3}",
                "{\"a\":1,\"b\":2,\"c\":[1,2,3,[4.1,4.2],5],\"d\":4}",
                "{\"a\":1,\"b\":[2.1,2.2],\"c\":3,\"d\":4,\"e\":[4.1,4.2,4.3,[4.41,4.42],4.4],\"f\":5}",
                "{\"a\":1,\"b\":[2.1,2.2,[[[{\"b1\":2.21}]]],2.3],\"c\":{\"d\":4,\"e\":[4.1,4.2,4.3,[4.41,4.42],4.4],\"f\":5}}"
            };

            foreach (string json in jsonValues)
            {
                JsonValue jv = JsonValue.Parse(json);
                Log.Info("{0}", jv.ToString());

                string jvstr = jv.ToString();
                Assert.Equal(json, jvstr);
            }
        }

        /// <summary>
        /// Negative tests for parsing "unbalanced" JSON (i.e., JSON documents which aren't properly closed).
        /// </summary>
        [Fact]
        public void ParseUnbalancedJsonTest()
        {
            string[] jsonValues =
            {
                "[",
                "[1,{]",
                "[1,2,3,{{}}",
                "}",
                "{\"a\":}",
                "{\"a\":1,\"b\":[,\"c\":3,\"d\":4}",
                "{\"a\":1,\"b\":[2,\"c\":3}",
                "{\"a\":1,\"b\":[2.1,2.2,\"c\":3,\"d\":4,\"e\":[4.1,4.2,4.3,[4.41,4.42],4.4],\"f\":5}",
                "{\"a\":1,\"b\":[2.1,2.2,[[[[{\"b1\":2.21}]]],\"c\":{\"d\":4,\"e\":[4.1,4.2,4.3,[4.41,4.42],4.4],\"f\":5}}"
            };

            foreach (string json in jsonValues)
            {
                Log.Info("Testing unbalanced JSON: {0}", json);
                ExpectException<FormatException>(() => JsonValue.Parse(json));
            }
        }

        /// <summary>
        /// Test for parsing a deeply nested JSON object.
        /// </summary>
        [Fact]
        public void ParseDeeplyNestedJsonObjectString()
        {
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('{');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                string key = i.ToString(CultureInfo.InvariantCulture);
                builderExpected.AppendFormat("\"{0}\":{{", key);
            }

            for (int i = 0; i < depth + 1; i++)
            {
                builderExpected.Append('}');
            }

            string json = builderExpected.ToString();
            JsonValue jsonValue = JsonValue.Parse(json);
            string jvstr = jsonValue.ToString();

            Assert.Equal(json, jvstr);
        }

        /// <summary>
        /// Test for parsing a deeply nested JSON array.
        /// </summary>
        [Fact]
        public void ParseDeeplyNestedJsonArrayString()
        {
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('[');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                builderExpected.Append('[');
            }

            for (int i = 0; i < depth + 1; i++)
            {
                builderExpected.Append(']');
            }

            string json = builderExpected.ToString();
            JsonValue jsonValue = JsonValue.Parse(json);
            string jvstr = jsonValue.ToString();

            Assert.Equal(json, jvstr);
        }

        /// <summary>
        /// Test for parsing a deeply nested JSON graph, containing both objects and arrays.
        /// </summary>
        [Fact]
        public void ParseDeeplyNestedJsonString()
        {
            StringBuilder builderExpected = new StringBuilder();
            builderExpected.Append('{');
            int depth = 10000;
            for (int i = 0; i < depth; i++)
            {
                string key = i.ToString(CultureInfo.InvariantCulture);
                builderExpected.AppendFormat("\"{0}\":[{{", key);
            }

            for (int i = 0; i < depth; i++)
            {
                builderExpected.Append("}]");
            }

            builderExpected.Append('}');

            string json = builderExpected.ToString();
            JsonValue jsonValue = JsonValue.Parse(json);
            string jvstr = jsonValue.ToString();

            Assert.Equal(json, jvstr);
        }

        internal static void ExpectException<T>(Action action) where T : Exception
        {
            ExpectException<T>(action, null);
        }

        internal static void ExpectException<T>(Action action, string partOfExceptionString) where T : Exception
        {
            try
            {
                action();
                Assert.False(true, "This should have thrown");
            }
            catch (T e)
            {
                if (partOfExceptionString != null)
                {
                    Assert.True(e.Message.Contains(partOfExceptionString));
                }
            }
        }

        internal class NonSeekableStream : Stream
        {
            Stream innerStream;

            public NonSeekableStream(Stream innerStream)
            {
                this.innerStream = innerStream;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException();
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.innerStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}