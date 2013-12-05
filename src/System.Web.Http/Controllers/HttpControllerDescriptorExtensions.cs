// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    internal static class HttpControllerDescriptorExtensions
    {
        public static bool HasRoutingAttribute(this HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException("controllerDescriptor");
            }

            return controllerDescriptor.GetCustomAttributes<IDirectRouteFactory>(inherit: false).Any()
                || controllerDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false).Any();
        }
    }
}
