// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.Owin;

namespace System.Web.Http.Owin
{
    internal static class OwinRequestExtensions
    {
        private const string ContentLengthHeaderName = "Content-Length";

        public static int? GetContentLength(this IOwinRequest request)
        {
            Contract.Assert(request != null);

            IHeaderDictionary headers = request.Headers;

            if (headers == null)
            {
                return null;
            }

            string[] values;

            if (!headers.TryGetValue(ContentLengthHeaderName, out values))
            {
                return null;
            }

            if (values == null || values.Length != 1)
            {
                return null;
            }

            string value = values[0];

            if (value == null)
            {
                return null;
            }

            int parsed;

            if (!Int32.TryParse(value, out parsed))
            {
                return null;
            }

            if (parsed < 0)
            {
                return null;
            }

            return parsed;
        }
    }
}
