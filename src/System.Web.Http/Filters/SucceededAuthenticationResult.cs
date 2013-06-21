// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Http.Filters
{
    /// <summary>Represents an authentication result that successfully authenticate a principal.</summary>
    public class SucceededAuthenticationResult : IAuthenticationResult
    {
        private readonly IPrincipal _principal;

        /// <summary>Initializes a new instance of the <see cref="SucceededAuthenticationResult"/> class.</summary>
        /// <param name="principal">The principal successfully authenticated.</param>
        public SucceededAuthenticationResult(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            _principal = principal;
        }

        /// <inheritdoc />
        public IPrincipal Principal
        {
            get { return _principal; }
        }

        /// <inheritdoc />
        public IHttpActionResult ErrorResult
        {
            get { return null; }
        }
    }
}
