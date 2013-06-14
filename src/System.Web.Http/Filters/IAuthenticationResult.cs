// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Web.Http.Controllers;

namespace System.Web.Http.Filters
{
    /// <summary>Defines an authentication result.</summary>
    /// <remarks>
    /// When <see cref="ErrorResult"/> is not <see langword="null"/>, authentication was attempted but failed. When
    /// <see cref="Principal"/> is not <see langword="null" />, authentication was attempted and succeeded. When the
    /// <see cref="IAuthenticationResult"/> instance is <see langword="null"/>, or when both <see cref="Principal"/>
    /// and <see cref="ErrorResult"/> are <see langword="null"/>, authentication was not attempted and the caller
    /// should continue without either reporting an error or setting a new principal.</remarks>
    public interface IAuthenticationResult
    {
        /// <summary>
        /// The authenticated principal, if authentication succeeded; otherwise, <see langword="null"/>.
        /// </summary>
        IPrincipal Principal { get; }

        /// <summary>
        /// The action result that will produce the error response, if authentication failed; otherwise,
        /// <see langword="null"/>.
        /// </summary>
        IHttpActionResult ErrorResult { get; }
    }
}
