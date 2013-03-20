// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    public class HttpAuthenticationContext
    {
        public HttpAuthenticationContext(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ActionContext = actionContext;
        }

        public HttpActionContext ActionContext { get; private set; }

        public IPrincipal Principal { get; internal set; }

        public HttpRequestMessage Request
        {
            get
            {
                return (ActionContext != null && ActionContext.Request != null) ? ActionContext.Request : null;
            }
        }
    }
}
