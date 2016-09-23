// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public interface IODataPathTemplateHandler
    {
        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="pathTemplate">The OData path template to parse.</param>
        /// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        ODataPathTemplate ParseTemplate(IEdmModel model, string pathTemplate);
    }
}
