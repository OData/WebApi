// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.OData.Routing
{
    internal class ODataValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return new ODataValueProvider(actionContext.Request.GetRoutingConventionsDataStore());
        }

        private class ODataValueProvider : NameValuePairsValueProvider
        {
            public ODataValueProvider(IDictionary<string, object> routeData)
                : base(routeData, CultureInfo.InvariantCulture)
            {
            }
        }
    }
}
