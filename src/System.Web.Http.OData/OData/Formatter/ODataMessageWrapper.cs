// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// Wrapper for IODataRequestMessage and IODataResponseMessage.
    /// </summary>
    internal class ODataMessageWrapper : IODataRequestMessage, IODataResponseMessage, IODataUrlResolver
    {
        private Stream _stream;
        private Dictionary<string, string> _headers;
        private IDictionary<string, string> _contentIdMapping;
        private static readonly Regex ContentIdReferencePattern = new Regex(@"\$\d", RegexOptions.Compiled);

        public ODataMessageWrapper()
            : this(stream: null, headers: null)
        {
        }

        public ODataMessageWrapper(Stream stream)
            : this(stream: stream, headers: null)
        {
        }

        public ODataMessageWrapper(Stream stream, HttpHeaders headers)
            : this(stream: stream, headers: headers, contentIdMapping: null)
        {
        }

        public ODataMessageWrapper(Stream stream, HttpHeaders headers, IDictionary<string, string> contentIdMapping)
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
            _contentIdMapping = contentIdMapping ?? new Dictionary<string, string>();
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
            _headers[headerName] = headerValue;
        }

        public Uri ResolveUrl(Uri baseUri, Uri payloadUri)
        {
            if (payloadUri == null)
            {
                throw new ArgumentNullException("payloadUri");
            }

            string originalPayloadUri = payloadUri.OriginalString;
            if (ContentIdReferencePattern.IsMatch(originalPayloadUri))
            {
                string resolvedUri = ContentIdHelpers.ResolveContentId(originalPayloadUri, _contentIdMapping);
                return new Uri(resolvedUri, UriKind.RelativeOrAbsolute);
            }

            // Returning null for default resolution.
            return null;
        }
    }
}