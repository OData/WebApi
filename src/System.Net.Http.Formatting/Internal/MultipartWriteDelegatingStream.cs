// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace System.Net.Http.Internal
{
    /// <summary>
    /// This stream synchronizes async writes on streams written to by <see cref="MultipartFileStreamProvider"/>.
    /// This works around a race condition that happens when using APM to write asynchronously to a 
    /// <see cref="FileStream"/>. The impact of the race condition is that the file may still be in 
    /// the process of being closed as control returns to the user causing an <see cref="IOException"/>
    /// to be thrown.
    /// </summary>
    internal class MultipartWriteDelegatingStream : DelegatingStream
    {
        public MultipartWriteDelegatingStream(Stream innerStream)
            : base(innerStream)
        {
        }

        [SuppressMessage("Microsoft.Web.FxCop", "MW1201:DoNotCallProblematicMethodsOnTaskRule", Justification = "FromAsync is used to get around a race condition.")]
        [SuppressMessage("Microsoft.Web.FxCop", "MW1202:DoNotUseProblematicTaskTypesRule", Justification = "FromAsync is used to get around a race condition.")]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Task writeTask = Task.Factory.FromAsync(InnerStream.BeginWrite, InnerStream.EndWrite, buffer, offset, count, state);
            if (callback != null)
            {
                return WriteAndCallbackAsync(writeTask, callback);
            }
            return writeTask;
        }

        private static async Task WriteAndCallbackAsync(Task writeTask, AsyncCallback callback)
        {
            try
            {
                await writeTask;
            }
            finally
            {
                callback(writeTask);
            }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
        }
    }
}
