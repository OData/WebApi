//-----------------------------------------------------------------------------
// <copyright file="IODataPathTemplateHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.OData.Routing.Template;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public interface IODataPathTemplateHandler
    {
        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        /// </summary>
        /// <param name="odataPathTemplate">The OData path template to parse.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        ODataPathTemplate ParseTemplate(string odataPathTemplate, IServiceProvider requestContainer);
    }
}
