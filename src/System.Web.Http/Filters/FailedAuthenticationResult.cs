// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace System.Web.Http.Filters
{
    /// <summary>Represents an authentication result that indicates a failed attempt to authenticate.</summary>
    public class FailedAuthenticationResult : IAuthenticationResult
    {
        private readonly IHttpActionResult _errorResult;

        /// <summary>Initializes a new instance of the <see cref="FailedAuthenticationResult"/> class.</summary>
        /// <param name="errorResult">
        /// The action result that, when executed, creates the error response message.
        /// </param>
        public FailedAuthenticationResult(IHttpActionResult errorResult)
        {
            if (errorResult == null)
            {
                throw new ArgumentNullException("errorResult");
            }

            _errorResult = errorResult;
        }

        /// <inheritdoc />
        public IPrincipal Principal
        {
            get { return null; }
        }

        /// <inheritdoc />
        public IHttpActionResult ErrorResult
        {
            get { return _errorResult; }
        }
    }
}
