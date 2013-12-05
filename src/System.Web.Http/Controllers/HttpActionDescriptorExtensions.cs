// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    internal static class HttpActionDescriptorExtensions
    {
        public static bool HasRoutingAttribute(this HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: false).Any()
                || actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false).Any();
        }
    }
}
