// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

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
            Func<NameValueCollection> formGetter;
            Func<NameValueCollection> queryStringGetter;
            ValidationUtility.GetUnvalidatedCollections(context, out formGetter, out queryStringGetter);

            return new UnvalidatedRequestValues(new HttpRequestWrapper(context.Request), formGetter, queryStringGetter);
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
