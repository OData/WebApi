// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataMediaTypeFormatters
    {
        /// <summary>
        /// Creates a set of media type formatters to handle OData.
        /// </summary>
        /// <param name="model">The data model the formatter will support.</param>
        /// <returns>A set of media type formatters to handle OData.</returns>
        public static IEnumerable<ODataMediaTypeFormatter> Create(IEdmModel model)
        {
            return new ODataMediaTypeFormatter[] { new ODataMediaTypeFormatter(model) };
        }
    }
}
