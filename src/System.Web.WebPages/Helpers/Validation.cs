// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Helpers
{
    public static class Validation
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request",
            Justification = "Parameter is only meant for making this show up as 'Request.Unvalidated()', which closely resembles FX45 syntax.")]
        public static UnvalidatedRequestValues Unvalidated(this HttpRequestBase request)
        {
            return Unvalidated((HttpRequest)null);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request",
            Justification = "Parameter is only meant for making this show up as 'Request.Unvalidated()', which closely resembles FX45 syntax.")]
        public static UnvalidatedRequestValues Unvalidated(this HttpRequest request)
        {
            // We don't actually need the request object; we'll get HttpContext.Current directly.
            HttpContext context = HttpContext.Current;
            return new UnvalidatedRequestValues(new HttpRequestWrapper(context.Request));
        }

        public static string Unvalidated(this HttpRequestBase request, string key)
        {
            return Unvalidated(request)[key];
        }

        public static string Unvalidated(this HttpRequest request, string key)
        {
            return Unvalidated(request)[key];
        }
    }
}