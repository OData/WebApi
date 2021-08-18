//-----------------------------------------------------------------------------
// <copyright file="ODataValueProviderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Routing
{
    internal class ODataValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            return new ODataValueProvider(actionContext.Request.ODataProperties().RoutingConventionsStore);
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
