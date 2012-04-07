// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Defines the provider responsible for documenting the service.
    /// </summary>
    public interface IDocumentationProvider
    {
        /// <summary>
        /// Gets the documentation based on <see cref="HttpActionDescriptor"/>.
        /// </summary>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>Documentation for the controller.</returns>
        string GetDocumentation(HttpActionDescriptor actionDescriptor);

        /// <summary>
        /// Gets the documentation based on <see cref="HttpParameterDescriptor"/>.
        /// </summary>
        /// <param name="parameterDescriptor">The parameter descriptor.</param>
        /// <returns>Documentation for the controller.</returns>
        string GetDocumentation(HttpParameterDescriptor parameterDescriptor);
    }
}
