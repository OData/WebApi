// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Owin
{
    /// <summary>Represents a stream that replaces another stream to prevent actually closing that stream.</summary>
    /// <remarks>
    /// This class uses the Decorator [GoF] pattern; it forwards all calls except those related to Dispose and Close.
    /// </remarks>
    internal sealed class NonOwnedStream : Stream
    {
        private readonly Stream _innerStream;

        private bool _disposed;

        public NonOwnedStream(Stream innerStream)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException("innerStream");
            }

            _innerStream = innerStream;
        }

        public override bool CanRead
        {
            get
            {
                // Per documentation, CanRead should return false rather than throw when the stream is closed.
                if (_disposed)
                {
                    return false;
                }

                return _innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                // Per documentation, CanRead should return false rather than throw when the stream is closed.
                if (_disposed)
                {
                    return false;
                }

                return _innerStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                // Per documentation, this value apparently is a constant for a particular implementation class.
                // Throwing when disposed appears inappropriate here.
                return _innerStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                // Per documentation, CanRead should return false rather than throw when the stream is closed.
                if (_disposed)
                {
                    return false;
                }

                return _innerStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                return _innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                return _innerStream.Position;
            }
            set
            {
                // Per documentation, this property throws ObjectDisposedException if the stream is closed.
                ThrowIfDisposed();
                _innerStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                return _innerStream.ReadTimeout;
            }
            set
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                _innerStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                return _innerStream.WriteTimeout;
            }
            set
            {
                // Documentation does not state the behavior when the stream is closed. The NetworkStream
                // implementation suggests the contract should be to throw ObjectDisposedException when the stream is
                // closed.
                ThrowIfDisposed();
                _innerStream.WriteTimeout = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return _innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            // base.Close() calls Dispose(true) and GC.SuppressFinalize(this), which is exactly what we want.
            // Note that we do NOT call _innerStream.Close here, as that would actually close the original source
            // stream, which is the one thing this class is designed to prevent.
            base.Close();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        // Not overriding MarshalByRefObj.CreateObjRef.

        // Not overriding Stream.CreateWaitHandle.

        protected override void Dispose(bool disposing)
        {
            // Note that we do NOT call _innerStream.Dispose or Close here, as that would actually close the original
            // source stream, which is the one thing this class is designed to prevent.

            if (!_disposed)
            {
                base.Dispose(disposing);
                _disposed = true;
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return _innerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            _innerStream.EndWrite(asyncResult);
        }

        // Not overriding Object.Equals.

        public override void Flush()
        {
            ThrowIfDisposed();
            _innerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _innerStream.FlushAsync(cancellationToken);
        }

        // Not overriding Object.GetHashCode.

        // Not overriding MarshalByRefObj.InitializeLifetimeService.

        // Per documentation, don't override Stream.ObjectInvariant.

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return _innerStream.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            return _innerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            _innerStream.SetLength(value);
        }

        // Not overriding Object.ToString().

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            _innerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            _innerStream.WriteByte(value);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
