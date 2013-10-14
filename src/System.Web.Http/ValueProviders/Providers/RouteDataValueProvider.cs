// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProvider : NameValuePairsValueProvider
    {
        public RouteDataValueProvider(HttpActionContext actionContext, CultureInfo culture)
            : base(actionContext.ControllerContext.RouteData.Values, culture)
        {
        }
    }
}
