// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProvider : NameValuePairsValueProvider
    {
        public QueryStringValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(() => ParseQueryString(actionContext.ControllerContext.Request.RequestUri), culture)
        {
        }

        internal static IEnumerable<KeyValuePair<string, string>> ParseQueryString(Uri uri)
        {
            // Unit tests may not always provide a Uri in the request
            if (uri == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            // Uri --> FormData --> NVC
            FormDataCollection formData = new FormDataCollection(uri);
            return formData.GetJQueryNameValuePairs();     
        }
    }
}
