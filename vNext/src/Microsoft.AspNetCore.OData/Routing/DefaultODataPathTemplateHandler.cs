// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Parses an OData path as an <see cref="ODataPath"/> and converts an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public class DefaultODataPathTemplateHandler : IODataPathTemplateHandler
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataPathTemplateHandler" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider from DI.</param>
        public DefaultODataPathTemplateHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/> that can be matched to an <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="pathTemplate">The OData path template to parse.</param>
        /// <returns>A parsed representation of the path template, or <c>null</c> if the path does not match the model.</returns>
        public virtual ODataPathTemplate ParseTemplate(IEdmModel model, string pathTemplate)
        {
            if (pathTemplate == null)
            {
                throw Error.ArgumentNull("pathTemplate");
            }

            return Templatify(ODataPathParserHelper.Parse(model, null, pathTemplate, true, _serviceProvider), pathTemplate);
        }

        private static ODataPathTemplate Templatify(ODataPath path, string pathTemplate)
        {
            if (path == null)
            {
                throw new ODataException(Error.Format(SRResources.InvalidODataPathTemplate, pathTemplate));
            }

            ODataPathSegmentTemplateTranslator translator = new ODataPathSegmentTemplateTranslator();
            var newPath = path.Segments.Select(e =>
            {
                UnresolvedPathSegment unresolvedPathSegment = e as UnresolvedPathSegment;
                if (unresolvedPathSegment != null)
                {
                    throw new ODataException(
                           Error.Format(SRResources.UnresolvedPathSegmentInTemplate, unresolvedPathSegment, pathTemplate));
                }

                return e.TranslateWith(translator);
            });

            return new ODataPathTemplate(newPath);
        }
    }
}
