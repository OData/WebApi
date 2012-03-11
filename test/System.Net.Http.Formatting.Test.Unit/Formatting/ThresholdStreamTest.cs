using System.IO;
using System.Net.Http.Internal;
using System.Text;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class ThresholdStreamTests
    {
        const string StringData = "abc,,,,,def";
        const int PassThreshold = 40;
        const int FailThreshold = 4;

        [Fact]
        public void HitLimit()
        {
            Stream inner = new MemoryStream(Encoding.UTF8.GetBytes(StringData));

            ThresholdStream s = new ThresholdStream(inner, (byte)',', FailThreshold);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    // Attempt to read the stream. This should cross the limit set above and throw. 
                    string content = new StreamReader(s).ReadToEnd();
                });
        }

        [Fact]
        public void NoHitLimit()
        {
            Stream inner = new MemoryStream(Encoding.UTF8.GetBytes(StringData));
            ThresholdStream s = new ThresholdStream(inner, (byte)',', PassThreshold);

            string actual = new StreamReader(s).ReadToEnd();

            Assert.Equal(StringData, actual);
        }

        [Fact]
        public void HitLimitAsync()
        {
            // Arrange
            byte[] dataBytes = Encoding.UTF8.GetBytes(StringData);
            Stream inner = new AsyncTestStream(new MemoryStream(dataBytes));
            ThresholdStream s = new ThresholdStream(inner, (byte)',', FailThreshold);
            byte[] buffer = new byte[StringData.Length];

            // Act
            IAsyncResult r = s.BeginRead(buffer, 0, buffer.Length, null, null);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    s.EndRead(r);
                });
        }

        [Fact]
        public void NoHitLimitAsync()
        {
            // Arrange
            byte[] dataBytes = Encoding.UTF8.GetBytes(StringData);
            Stream inner = new AsyncTestStream(new MemoryStream(dataBytes));
            ThresholdStream s = new ThresholdStream(inner, (byte)',', PassThreshold);
            byte[] buffer = new byte[StringData.Length];

            // Act
            IAsyncResult r = s.BeginRead(buffer, 0, buffer.Length, null, null);
            s.EndRead(r);


            // Assert
            string actual = new StreamReader(new MemoryStream(buffer)).ReadToEnd();
            Assert.Equal(actual, StringData);
        }

        // test-only Stream for verifying sync Read operations aren't called.
        class AsyncTestStream : DelegatingStream
        {
            delegate int ReadDelegate(byte[] bytes, int index, int offset);
            ReadDelegate _read;

            public AsyncTestStream(Stream inner)
                : base(inner)
            {
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new InvalidOperationException("don't use sync apis");
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                Assert.Null(_read);
                _read = _innerStream.Read;
                return _read.BeginInvoke(buffer, offset, count, callback, state);
            }
            public override int EndRead(IAsyncResult asyncResult)
            {
                Assert.NotNull(_read);
                try
                {
                    return _read.EndInvoke(asyncResult);
                }
                finally
                {
                    _read = null;
                }
            }
        }
    }
}