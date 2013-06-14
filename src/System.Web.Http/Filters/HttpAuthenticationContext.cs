// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>Represents an authentication context containing information for executing authentication.</summary>
    public class HttpAuthenticationContext
    {
        /// <summary>Initializes a new instance of the <see cref="HttpAuthenticationContext"/> class.</summary>
        /// <param name="actionContext">The action context.</param>
        public HttpAuthenticationContext(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            ActionContext = actionContext;
        }

        /// <summary>Gets the action context.</summary>
        public HttpActionContext ActionContext { get; private set; }

        /// <summary>Gets the principal currently authenticated.</summary>
        public IPrincipal Principal { get; internal set; }

        /// <summary>Gets the request message.</summary>
        public HttpRequestMessage Request
        {
            get
            {
                return (ActionContext != null && ActionContext.Request != null) ? ActionContext.Request : null;
            }
        }
    }
}
