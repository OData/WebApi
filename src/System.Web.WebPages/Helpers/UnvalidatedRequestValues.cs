// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;

namespace System.Web.Helpers
{
    // Provides access to Request.* collections, except that these have not gone through request validation.
    [Obsolete("Use System.Web.HttpRequest.Unvalidated instead.")]
    public sealed class UnvalidatedRequestValues
    {
        private readonly HttpRequestBase _request;

        internal UnvalidatedRequestValues(HttpRequestBase request)
        {
            _request = request;
        }

        public NameValueCollection Form
        {
            get { return _request.Unvalidated.Form; }
        }

        public NameValueCollection QueryString
        {
            get { return _request.Unvalidated.QueryString; }
        }

        public string this[string key]
        {
            get
            {
                return _request.Unvalidated[key];
            }
        }
    }
}