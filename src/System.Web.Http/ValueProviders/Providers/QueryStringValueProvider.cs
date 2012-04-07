// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProvider : NameValueCollectionValueProvider
    {
        public QueryStringValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(() => ParseQueryString(actionContext.ControllerContext.Request.RequestUri), culture)
        {
        }

        internal static NameValueCollection ParseQueryString(Uri uri)
        {
            // Unit tests may not always provide a Uri in the request
            if (uri == null)
            {
                return new NameValueCollection();
            }

            // Uri --> FormData --> NVC
            FormDataCollection formData = new FormDataCollection(uri);
            NameValueCollection nvc = formData.GetJQueryValueNameValueCollection();
            return nvc;            
        }
    }
}
