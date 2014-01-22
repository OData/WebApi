// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public interface IODataPathTemplateHandler
    {
        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        /// </summary>
        /// <param name="model">The model to use for path template parsing.</param>
        /// <param name="odataPathTemplate">The OData path template to parse.</param>
        /// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        ODataPathTemplate ParseTemplate(IEdmModel model, string odataPathTemplate);
    }
}
