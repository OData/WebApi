// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Http.Filters
{
    public class FailedAuthenticationResult : IAuthenticationResult
    {
        private readonly IHttpActionResult _errorResult;

        public FailedAuthenticationResult(IHttpActionResult errorResult)
        {
            if (errorResult == null)
            {
                throw new ArgumentNullException("errorResult");
            }

            _errorResult = errorResult;
        }

        public IPrincipal Principal
        {
            get { return null; }
        }

        public IHttpActionResult ErrorResult
        {
            get { return _errorResult; }
        }
    }
}
