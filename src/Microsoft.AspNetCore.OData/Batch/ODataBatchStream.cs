//-----------------------------------------------------------------------------
// <copyright file="ODataBatchStream.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Batch
{
    // Normally, the stream is closed when the ODataMessageWriter is disposed.
    // For responses within a batch, we need to read from the stream after the response is
    // written in order to write it to the batch response stream. So we need to ignore the Close()
    // and provide an alternate method to release the stream after writing its content to the batch response.
    internal class ODataBatchStream : MemoryStream
    {
        private bool isDisposed = false;

        /// <summary>
        /// Dispose the batch stream and underlying resources
        /// </summary>
        internal void InternalDispose()
        {
            if (!isDisposed)
            {
                base.Flush();
                base.Close();
                base.Dispose();
                isDisposed = true;
            }
        }

        /// <summary>
        /// Dispose the batch stream and underlying resources
        /// </summary>
        internal async Task InternalDisposeAsync()
        {
            if (!isDisposed)
            {
                await base.FlushAsync();
                base.Close();
                base.Dispose();
                isDisposed = true;
            }
        }

        /// <summary>
        /// Override Close() in order to hold the stream open until we are able to
        /// copy it to the batch response stream.
        /// </summary>
        public override void Close()
        {
        }
    }
}
