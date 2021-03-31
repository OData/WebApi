// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNet.OData.Batch
{
    internal class LazyStreamContent : HttpContent
    {
        private Func<Stream> _getStream;
        private StreamContent _streamContent;

        public LazyStreamContent(Func<Stream> getStream)
        {
            _getStream = getStream;
        }

        private StreamContent StreamContent
        {
            get
            {
                if (_streamContent == null)
                {
                    _streamContent = new StreamContent(_getStream());
                }

                return _streamContent;
            }
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return StreamContent.CopyToAsync(stream, context).ContinueWith((Task)=>StreamContent.Dispose());
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}