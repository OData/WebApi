// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Http.Filters
{
    public class SucceededAuthenticationResult : IAuthenticationResult
    {
        private readonly IPrincipal _principal;

        public SucceededAuthenticationResult(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            _principal = principal;
        }

        public IPrincipal Principal
        {
            get { return _principal; }
        }

        public IHttpActionResult ErrorResult
        {
            get { return null; }
        }
    }
}
