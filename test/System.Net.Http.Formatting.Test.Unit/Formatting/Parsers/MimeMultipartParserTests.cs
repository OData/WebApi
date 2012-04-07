// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting.Parsers
{
    public class MimeMultipartParserTests
    {
        [Fact]
        public void MimeMultipartParserTypeIsCorrect()
        {
            Assert.Type.HasProperties<InternetMessageFormatHeaderParser>(TypeAssert.TypeProperties.IsClass);
        }

        private static MimeMultipartParser CreateMimeMultipartParser(int maximumHeaderLength, string boundary)
        {
            return new MimeMultipartParser(boundary, maximumHeaderLength);
        }

        internal static byte[] CreateBuffer(string boundary, params string[] bodyparts)
        {
            return CreateBuffer(boundary, false, bodyparts);
        }

        internal static string CreateNestedBuffer(int count)
        {
            StringBuilder buffer = new StringBuilder("content");

            for (var cnt = 0; cnt < count; cnt++)
            {
                byte[] nested = CreateBuffer("N" + cnt.ToString(), buffer.ToString());
                var message = Encoding.UTF8.GetString(nested);
                buffer.Length = 0;
                buffer.AppendLine(message);
            }

            return buffer.ToString();
        }

        private static byte[] CreateBuffer(string boundary, bool withLws, params string[] bodyparts)
        {
            const string SP = " ";
            const string HTAB = "\t";
            const string CRLF = "\r\n";
            const string DashDash = "--";

            string lws = String.Empty;
            if (withLws)
            {
                lws = SP + SP + HTAB + SP;
            }

            StringBuilder message = new StringBuilder();
            message.Append(DashDash + boundary + lws + CRLF);
            for (var cnt = 0; cnt < bodyparts.Length; cnt++)
            {
                message.Append(bodyparts[cnt]);
                if (cnt < bodyparts.Length - 1)
                {
                    message.Append(CRLF + DashDash + boundary + lws + CRLF);
                }
            }

            // Note: We rely on a final CRLF even though it is not required by the BNF existing application do send it
            message.Append(CRLF + DashDash + boundary + DashDash + lws + CRLF);
            return Encoding.UTF8.GetBytes(message.ToString());
        }

        private static MimeMultipartParser.State ParseBufferInSteps(MimeMultipartParser parser, byte[] buffer, int readsize, out List<string> bodyParts, out int totalBytesConsumed)
        {
            MimeMultipartParser.State state = MimeMultipartParser.State.Invalid;
            totalBytesConsumed = 0;
            bodyParts = new List<string>();
            bool isFinal = false;
            byte[] currentBodyPart = new byte[32 * 1024];
            int currentBodyLength = 0;

            while (totalBytesConsumed <= buffer.Length)
            {
                int size = Math.Min(buffer.Length - totalBytesConsumed, readsize);
                byte[] parseBuffer = new byte[size];
                Buffer.BlockCopy(buffer, totalBytesConsumed, parseBuffer, 0, size);

                int bytesConsumed = 0;
                ArraySegment<byte> out1;
                ArraySegment<byte> out2;
                state = parser.ParseBuffer(parseBuffer, parseBuffer.Length, ref bytesConsumed, out out1, out out2, out isFinal);
                totalBytesConsumed += bytesConsumed;

                Buffer.BlockCopy(out1.Array, out1.Offset, currentBodyPart, currentBodyLength, out1.Count);
                currentBodyLength += out1.Count;

                Buffer.BlockCopy(out2.Array, out2.Offset, currentBodyPart, currentBodyLength, out2.Count);
                currentBodyLength += out2.Count;

                if (state == MimeMultipartParser.State.BodyPartCompleted)
                {
                    var bPart = new byte[currentBodyLength];
                    Buffer.BlockCopy(currentBodyPart, 0, bPart, 0, currentBodyLength);
                    bodyParts.Add(Encoding.UTF8.GetString(bPart));
                    currentBodyLength = 0;
                    if (isFinal)
                    {
                        break;
                    }
                }
                else if (state != MimeMultipartParser.State.NeedMoreData)
                {
                    return state;
                }
            }

            Assert.True(isFinal);
            return state;
        }

        public static IEnumerable<object[]> Boundaries
        {
            get { return ParserData.Boundaries; }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MimeMultipartParserConstructorTest(string boundary)
        {
            MimeMultipartParser parser = new MimeMultipartParser(boundary, ParserData.MinMessageSize);
            Assert.NotNull(parser);

            Assert.ThrowsArgumentGreaterThanOrEqualTo(() => new MimeMultipartParser("-", ParserData.MinMessageSize - 1),
                "maxMessageSize", ParserData.MinMessageSize.ToString(), ParserData.MinMessageSize - 1);

            foreach (string empty in TestData.EmptyStrings)
            {
                Assert.ThrowsArgument(() => { new MimeMultipartParser(empty, ParserData.MinMessageSize); }, "boundary", allowDerivedExceptions: true);
            }

            Assert.ThrowsArgument(() => { new MimeMultipartParser("trailingspace ", ParserData.MinMessageSize); }, "boundary");

            Assert.ThrowsArgumentNull(() => { new MimeMultipartParser(null, ParserData.MinMessageSize); }, "boundary");
        }


        [Fact]
        public void MultipartParserNullBuffer()
        {
            MimeMultipartParser parser = CreateMimeMultipartParser(128, "-");
            Assert.NotNull(parser);

            int bytesConsumed = 0;
            ArraySegment<byte> out1;
            ArraySegment<byte> out2;
            bool isFinal;
            Assert.ThrowsArgumentNull(() => { parser.ParseBuffer(null, 0, ref bytesConsumed, out out1, out out2, out isFinal); }, "buffer");
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserEmptyBuffer(string boundary)
        {
            byte[] data = CreateBuffer(boundary);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);
                Assert.Equal(0, bodyParts[1].Length);
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserSingleShortBodyPart(string boundary)
        {

            byte[] data = CreateBuffer(boundary, "A");

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);
                Assert.Equal(1, bodyParts[1].Length);
                Assert.Equal("A", bodyParts[1]);
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserMultipleShortBodyParts(string boundary)
        {
            string[] text = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            byte[] data = CreateBuffer(boundary, text);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(text.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < text.Length; check++)
                {
                    Assert.Equal(1, bodyParts[check + 1].Length);
                    Assert.Equal(text[check], bodyParts[check + 1]);
                }
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserMultipleShortBodyPartsWithLws(string boundary)
        {
            string[] text = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            byte[] data = CreateBuffer(boundary, true, text);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(text.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < text.Length; check++)
                {
                    Assert.Equal(1, bodyParts[check + 1].Length);
                    Assert.Equal(text[check], bodyParts[check + 1]);
                }
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserSingleLongBodyPart(string boundary)
        {
            const string text = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

            byte[] data = CreateBuffer(boundary, text);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(2, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                Assert.Equal(text.Length, bodyParts[1].Length);
                Assert.Equal(text, bodyParts[1]);
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserMultipleLongBodyParts(string boundary)
        {
            const string middleText = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            string[] text = new string[] { 
                "A" + middleText + "A", 
                "B" + middleText + "B", 
                "C" + middleText + "C", 
                "D" + middleText + "D", 
                "E" + middleText + "E", 
                "F" + middleText + "F", 
                "G" + middleText + "G", 
                "H" + middleText + "H", 
                "I" + middleText + "I", 
                "J" + middleText + "J", 
                "K" + middleText + "K", 
                "L" + middleText + "L", 
                "M" + middleText + "M", 
                "N" + middleText + "N", 
                "O" + middleText + "O", 
                "P" + middleText + "P", 
                "Q" + middleText + "Q", 
                "R" + middleText + "R", 
                "S" + middleText + "S", 
                "T" + middleText + "T", 
                "U" + middleText + "U", 
                "V" + middleText + "V", 
                "W" + middleText + "W", 
                "X" + middleText + "X", 
                "Y" + middleText + "Y", 
                "Z" + middleText + "Z"};

            byte[] data = CreateBuffer(boundary, text);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(text.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < text.Length; check++)
                {
                    Assert.Equal(text[check].Length, bodyParts[check + 1].Length);
                    Assert.Equal(text[check], bodyParts[check + 1]);
                }
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserNearMatches(string boundary)
        {
            const string CR = "\r";
            const string CRLF = "\r\n";
            const string Dash = "-";
            const string DashDash = "--";

            string[] text = new string[] { 
                CR + Dash + "AAA",
                CRLF + Dash + "AAA",
                CRLF + DashDash + "AAA" + CR + "AAA",
                CRLF,
                "AAA",
                "AAA" + CRLF,
                CRLF + CRLF,
                CRLF + CRLF + CRLF,
                "AAA" + DashDash + "AAA",
                CRLF + "AAA" + DashDash + "AAA" + DashDash,
                CRLF + DashDash + "AAA" + CRLF, 
                CRLF + DashDash + "AAA" + CRLF + CRLF, 
                CRLF + DashDash + "AAA" + DashDash + CRLF, 
                CRLF + DashDash + "AAA" + DashDash + CRLF + CRLF
            };

            byte[] data = CreateBuffer(boundary, text);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(text.Length + 1, bodyParts.Count);
                Assert.Equal(0, bodyParts[0].Length);

                for (var check = 0; check < text.Length; check++)
                {
                    Assert.Equal(text[check].Length, bodyParts[check + 1].Length);
                    Assert.Equal(text[check], bodyParts[check + 1]);
                }
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MultipartParserNesting(string boundary)
        {
            for (var nesting = 0; nesting < 16; nesting++)
            {
                string nested = CreateNestedBuffer(nesting);

                byte[] data = CreateBuffer(boundary, nested);

                for (var cnt = 1; cnt <= data.Length; cnt++)
                {
                    MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                    Assert.NotNull(parser);

                    int totalBytesConsumed;
                    List<string> bodyParts;
                    MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                    Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                    Assert.Equal(data.Length, totalBytesConsumed);

                    Assert.Equal(2, bodyParts.Count);
                    Assert.Equal(0, bodyParts[0].Length);
                    Assert.Equal(nested.Length, bodyParts[1].Length);
                }
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MimeMultipartParserTestDataTooBig(string boundary)
        {
            byte[] data = CreateBuffer(boundary);

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(ParserData.MinMessageSize, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.DataTooBig, state);
                Assert.Equal(ParserData.MinMessageSize, totalBytesConsumed);
            }
        }

        [Theory]
        [PropertyData("Boundaries")]
        public void MimeMultipartParserTestMultipartContent(string boundary)
        {
            MultipartContent content = new MultipartContent("mixed", boundary);
            content.Add(new StringContent("A"));
            content.Add(new StringContent("B"));
            content.Add(new StringContent("C"));

            MemoryStream memStream = new MemoryStream();
            content.CopyToAsync(memStream).Wait();
            memStream.Position = 0;
            byte[] data = memStream.ToArray();

            for (var cnt = 1; cnt <= data.Length; cnt++)
            {
                MimeMultipartParser parser = CreateMimeMultipartParser(data.Length, boundary);
                Assert.NotNull(parser);

                int totalBytesConsumed;
                List<string> bodyParts;
                MimeMultipartParser.State state = ParseBufferInSteps(parser, data, cnt, out bodyParts, out totalBytesConsumed);
                Assert.Equal(MimeMultipartParser.State.BodyPartCompleted, state);
                Assert.Equal(data.Length, totalBytesConsumed);

                Assert.Equal(4, bodyParts.Count);
                Assert.Empty(bodyParts[0]);

                Assert.True(bodyParts[1].EndsWith("A"));
                Assert.True(bodyParts[2].EndsWith("B"));
                Assert.True(bodyParts[3].EndsWith("C"));
            }
        }
    }
}