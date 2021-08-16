//-----------------------------------------------------------------------------
// <copyright file="ODataMessageWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.OData;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    internal class ODataMessageWrapper : IODataRequestMessage, IODataResponseMessage, IDisposable
    {
        private Stream _stream;
        private Dictionary<string, string> _headers;

        public ODataMessageWrapper()
            : this(stream: null, headers: null)
        {
        }

        public ODataMessageWrapper(Stream stream)
            : this(stream: stream, headers: null)
        {
        }

        public ODataMessageWrapper(Stream stream, HttpHeaders headers)
        {
            _stream = stream;
            if (headers != null)
            {
                _headers = headers.ToDictionary(kvp => kvp.Key, kvp => String.Join(";", kvp.Value));
            }
            else
            {
                _headers = new Dictionary<string, string>();
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                return _headers;
            }
        }

        public string Method
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri Url
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int StatusCode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GetHeader(string headerName)
        {
            string value;
            if (_headers.TryGetValue(headerName, out value))
            {
                return value;
            }

            return null;
        }

        public Stream GetStream()
        {
            return _stream;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            _headers.Add(headerName, headerValue);
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
