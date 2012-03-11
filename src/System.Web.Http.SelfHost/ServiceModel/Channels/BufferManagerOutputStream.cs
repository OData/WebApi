using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.Common;

namespace System.Web.Http.SelfHost.ServiceModel.Channels
{
    internal class BufferManagerOutputStream : BufferedOutputStream
    {
        private string _quotaExceededString;

        public BufferManagerOutputStream(string quotaExceededString)
        {
            _quotaExceededString = quotaExceededString;
        }

        public BufferManagerOutputStream(string quotaExceededString, int maxSize)
            : base(maxSize)
        {
            _quotaExceededString = quotaExceededString;
        }

        // ALTERED_FOR_PORT:
        // We're not getting the internal buffer manager as we do in the framework but just wrapping the bufferManager
        public BufferManagerOutputStream(string quotaExceededString, int initialSize, int maxSize, BufferManager bufferManager)
            : base(initialSize, maxSize, GetInternalBufferManager(bufferManager))
        {
            _quotaExceededString = quotaExceededString;
        }

        public void Init(int initialSize, int maxSizeQuota, BufferManager bufferManager)
        {
            Init(initialSize, maxSizeQuota, maxSizeQuota, bufferManager);
        }

        public void Init(int initialSize, int maxSizeQuota, int effectiveMaxSize, BufferManager bufferManager)
        {
            // ALTERED_FOR_PORT:
            // We're not getting the internal buffer manager as we do in the framework but just wrapping the bufferManager
            Reinitialize(initialSize, maxSizeQuota, effectiveMaxSize, GetInternalBufferManager(bufferManager));
        }

        protected override Exception CreateQuotaExceededException(int maxSizeQuota)
        {
            string excMsg = Error.Format(_quotaExceededString, maxSizeQuota);
            return new QuotaExceededException(excMsg);
        }

        private static InternalBufferManager GetInternalBufferManager(BufferManager bufferManager)
        {
            Debug.Assert(bufferManager != null, "The 'bufferManager' parameter should not be null.");

            return new WrappingInternalBufferManager(bufferManager);
        }

        private class WrappingInternalBufferManager : InternalBufferManager
        {
            private BufferManager _bufferManager;

            public WrappingInternalBufferManager(BufferManager bufferManager)
            {
                Debug.Assert(bufferManager != null, "The 'bufferManager' parameter should not be null.");

                _bufferManager = bufferManager;
            }

            public override byte[] TakeBuffer(int bufferSize)
            {
                return _bufferManager.TakeBuffer(bufferSize);
            }

            public override void ReturnBuffer(byte[] buffer)
            {
                _bufferManager.ReturnBuffer(buffer);
            }

            public override void Clear()
            {
                _bufferManager.Clear();
            }
        }
    }
}
