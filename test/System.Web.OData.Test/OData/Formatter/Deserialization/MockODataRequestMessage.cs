// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Deserialization
{
    internal class MockODataRequestMessage : IODataRequestMessage
    {
        Dictionary<string, string> _headers;
        MemoryStream _body;

        public MockODataRequestMessage()
        {
            _headers = new Dictionary<string, string>();
            _body = new MemoryStream();
        }

        public MockODataRequestMessage(MockODataRequestMessage requestMessage)
        {
            _headers = new Dictionary<string, string>(requestMessage._headers);
            _body = new MemoryStream(requestMessage._body.ToArray());
        }

        public string GetHeader(string headerName)
        {
            string value;
            _headers.TryGetValue(headerName, out value);
            return value;
        }

        public Stream GetStream()
        {
            return _body;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _headers; }
        }

        public string Method
        {
            get
            {
                return "GET";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            _headers[headerName] = headerValue;
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
    }
}
