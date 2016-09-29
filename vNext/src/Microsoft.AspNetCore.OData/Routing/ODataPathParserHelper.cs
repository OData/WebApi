// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODL = Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Helper methods to parser the odata path.
    /// </summary>
    public static class ODataPathParserHelper
    {
        /// <summary>
        /// Parse the odata path.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="serviceRoot">The service root of the OData path.</param>
        /// <param name="odataPath">The OData path to parse.</param>
        /// <param name="template">The flag indicates whether the path is template or not.</param>
        /// <param name="serviceProvider">The service proivder.</param>
        /// <returns>A parsed representation of the path, or <c>null</c> if the path does not match the model.</returns>
        public static ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath, bool template,
            IServiceProvider serviceProvider)
        {
            ODL.ODataUriParser uriParser;
            Uri serviceRootUri = null;
            Uri fullUri = null;
            // TODO: Replace this type.
            //NameValueCollection queryString = null;

            ODataOptions options = serviceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;

            if (template)
            {
                uriParser = new ODL.ODataUriParser(model, new Uri(odataPath, UriKind.Relative), serviceProvider);
                uriParser.EnableUriTemplateParsing = true;
            }
            else
            {
                Contract.Assert(serviceRoot != null);

                serviceRootUri = new Uri(
                    serviceRoot.EndsWith("/", StringComparison.Ordinal) ?
                        serviceRoot :
                        serviceRoot + "/");

                fullUri = new Uri(serviceRootUri, odataPath);
                //queryString = fullUri.ParseQueryString();
                uriParser = new ODL.ODataUriParser(model, serviceRootUri, fullUri, serviceProvider);
            }

            if (options.UrlKeyDelimiter != null)
            {
                uriParser.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }
            else
            {
                // ODL changes to use ODataUrlKeyDelimiter.Slash as default value.
                // Web API still uses the ODataUrlKeyDelimiter.Parentheses as default value.
                // Please remove it after fix: https://github.com/OData/odata.net/issues/642
                uriParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses;
            }

            ODL.ODataPath path;
            UnresolvedPathSegment unresolvedPathSegment = null;
            ODL.KeySegment id = null;
            try
            {
                path = uriParser.ParsePath();
            }
            catch (ODL.ODataUnrecognizedPathException ex)
            {
                if (ex.ParsedSegments != null &&
                    ex.ParsedSegments.Any() &&
                    (ex.ParsedSegments.Last().EdmType is IEdmComplexType ||
                     ex.ParsedSegments.Last().EdmType is IEdmEntityType) &&
                    ex.CurrentSegment != ODataSegmentKinds.Count)
                {
                    if (!ex.UnparsedSegments.Any())
                    {
                        path = new ODL.ODataPath(ex.ParsedSegments);
                        unresolvedPathSegment = new UnresolvedPathSegment(ex.CurrentSegment);
                    }
                    else
                    {
                        // Throw ODataException if there is some segment following the unresolved segment.
                        throw new ODataException(Error.Format(
                            SRResources.InvalidPathSegment,
                            ex.UnparsedSegments.First(),
                            ex.CurrentSegment));
                    }
                }
                else
                {
                    throw;
                }
            }

            if (!template && path.LastSegment is ODL.NavigationPropertyLinkSegment)
            {
                IEdmCollectionType lastSegmentEdmType = path.LastSegment.EdmType as IEdmCollectionType;

                if (lastSegmentEdmType != null)
                {
                    ODL.EntityIdSegment entityIdSegment = null;
                    bool exceptionThrown = false;

                    try
                    {
                        entityIdSegment = uriParser.ParseEntityId();

                        if (entityIdSegment != null)
                        {
                            // Create another ODataUriParser to parse $id, which is absolute or relative.
                            ODL.ODataUriParser parser = new ODL.ODataUriParser(model, serviceRootUri, entityIdSegment.Id);
                            id = parser.ParsePath().LastSegment as ODL.KeySegment;
                        }
                    }
                    catch (ODataException)
                    {
                        // Exception was thrown while parsing the $id.
                        // We will throw another exception about the invalid $id.
                        exceptionThrown = true;
                    }

                    if (exceptionThrown ||
                        (entityIdSegment != null &&
                            (id == null ||
                                !(id.EdmType.IsOrInheritsFrom(lastSegmentEdmType.ElementType.Definition) ||
                                  lastSegmentEdmType.ElementType.Definition.IsOrInheritsFrom(id.EdmType)))))
                    {
                        throw new ODataException(Error.Format(SRResources.InvalidDollarId, "$id"/*queryString.Get("$id")*/));
                    }
                }
            }

            // do validation for the odata path
            path.WalkWith(new DefaultODataPathValidator(model));

            // do segment translator (for example parameter alias, key & function parameter template, etc)
            var segments =
                ODataPathSegmentTranslator.Translate(model, path, uriParser.ParameterAliasNodes).ToList();

            if (unresolvedPathSegment != null)
            {
                segments.Add(unresolvedPathSegment);
            }

            if (!template)
            {
                AppendIdForRef(segments, id);
            }

            return new ODataPath(segments)
            {
                ODLPath = path
            };
        }

        // We need to append the key value path segment from $id.
        private static void AppendIdForRef(IList<ODL.ODataPathSegment> segments, ODL.KeySegment id)
        {
            if (id == null || !(segments.Last() is ODL.NavigationPropertyLinkSegment))
            {
                return;
            }

            segments.Add(id);
        }
    }
}
