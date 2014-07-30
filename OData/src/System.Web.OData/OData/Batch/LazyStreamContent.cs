// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Web.OData.Batch
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
            return StreamContent.CopyToAsync(stream, context);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}