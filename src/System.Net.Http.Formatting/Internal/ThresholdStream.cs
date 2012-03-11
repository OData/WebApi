using System.IO;

namespace System.Net.Http.Internal
{
    /// <summary>
    /// Wraps a stream and limit the number of certain bytes that pass through. 
    /// This is intended to mitigate against DOS attacks where input is constructed to blow up hashtables. 
    /// We want a cheap way to limit the number of keys in the object that will be deserialized from the stream.
    /// We don't want to maintain a parser's state machine, so we need rough approximation. Naively, we could limit the total number of 
    /// bytes in the stream, but that's too harsh (it would prevent too many safe cases). 
    /// Instead, we count number of delimiter characters (where delimiter is set by the caller. Eg, a ',' for JSON). 
    /// </summary>
    internal class ThresholdStream : DelegatingStream
    {
        public const int DefaultDelimeterThreshold = 1000;

        private readonly byte _delimiter;
        private readonly int _threshold;
        private int _counter;

        public ThresholdStream(Stream inner, byte delimeter, int threshold = DefaultDelimeterThreshold)
            : base(inner)
        {
            _delimiter = delimeter;
            _threshold = threshold;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var countRead = base.Read(buffer, offset, count);
            CheckBytes(buffer, offset, countRead);
            return countRead;
        }

        public override int ReadByte()
        {
            int value = base.ReadByte();
            CheckByte((byte)value);
            return value;
        }

        private void CheckByte(byte b)
        {
            if (b == _delimiter)
            {
                _counter++;
                if (_counter > _threshold)
                {
                    throw new InvalidOperationException(Properties.Resources.InputStreamHasTooManyDelimiters);
                }
            }
        }

        private void CheckBytes(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                CheckByte(buffer[i]);
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            IAsyncResult inner = base.BeginRead(buffer, offset, count, callback, state);

            Action<int> verifyAction = (countRead) => CheckBytes(buffer, offset, countRead);
            return new AsyncResultWithExtraData<Action<int>>(inner, verifyAction);
        }

        // EndRead must be called for every BeginRead.
        public override int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            AsyncResultWithExtraData<Action<int>> result = (AsyncResultWithExtraData<Action<int>>)asyncResult;
            int countRead = base.EndRead(result.Inner);

            Action<int> verifyAction = result.ExtraData;
            verifyAction(countRead);

            return countRead;
        }
    }
}
