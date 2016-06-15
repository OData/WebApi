// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODL = Microsoft.OData.UriParser;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Parses an OData path as an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public class DefaultODataPathHandler : IODataPathHandler, IODataPathTemplateHandler, IODataUriResolver
    {
        /// <summary>
        /// Gets or sets the resolver for Uri parsing
        /// </summary>
        public ODataUriResolver UriResolver { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlConventions"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        public ODataUrlConventions UrlConventions { get; set; }

        /// <summary>
        /// Parses the specified OData path as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="serviceRoot">The service root of the OData path.</param>
        /// <param name="odataPath">The OData path to parse.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>A parsed representation of the path, or <c>null</c> if the path does not match the model.</returns>
        public virtual ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath,
            IServiceProvider requestContainer)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (serviceRoot == null)
            {
                throw Error.ArgumentNull("serviceRoot");
            }

            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (requestContainer == null)
            {
                throw Error.ArgumentNull("requestContainer");
            }

            return Parse(model, serviceRoot, odataPath, requestContainer, template: false);
        }

        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/> that can be matched to an <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="odataPathTemplate">The OData path template to parse.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>A parsed representation of the path template, or <c>null</c> if the path does not match the model.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        public virtual ODataPathTemplate ParseTemplate(IEdmModel model, string odataPathTemplate,
            IServiceProvider requestContainer)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (odataPathTemplate == null)
            {
                throw Error.ArgumentNull("odataPathTemplate");
            }

            if (requestContainer == null)
            {
                throw Error.ArgumentNull("requestContainer");
            }

            return Templatify(Parse(model, serviceRoot: null, odataPath: odataPathTemplate,
                requestContainer: requestContainer, template: true), odataPathTemplate);
        }

        /// <summary>
        /// Converts an instance of <see cref="ODataPath" /> into an OData link.
        /// </summary>
        /// <param name="path">The OData path to convert into a link.</param>
        /// <returns>
        /// The generated OData link.
        /// </returns>
        public virtual string Link(ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            return path.ToString();
        }

        private ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath, IServiceProvider requestContainer, bool template)
        {
            ODataUriParser uriParser;
            Uri serviceRootUri = null;
            Uri fullUri = null;
            NameValueCollection queryString = null;

            if (template)
            {
                uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative), requestContainer);
                uriParser.EnableUriTemplateParsing = true;
            }
            else
            {
                Contract.Assert(serviceRoot != null);

                serviceRootUri = new Uri(
                    serviceRoot.EndsWith("/", StringComparison.Ordinal)
                        ? serviceRoot
                        : serviceRoot + "/");

                fullUri = new Uri(serviceRootUri, odataPath);
                queryString = fullUri.ParseQueryString();
                uriParser = new ODataUriParser(model, serviceRootUri, fullUri, requestContainer);
            }

            if (UriResolver != null)
            {
                uriParser.Resolver = UriResolver;
            }

            if (UrlConventions != null)
            {
                uriParser.UrlConventions = UrlConventions;
            }

            ODL.ODataPath path;
            UnresolvedPathSegment unresolvedPathSegment = null;
            ODL.KeySegment id = null;
            try
            {
                path = uriParser.ParsePath();
            }
            catch (ODataUnrecognizedPathException ex)
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
                            ODataUriParser parser = new ODataUriParser(model, serviceRootUri, entityIdSegment.Id, requestContainer);
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
                        throw new ODataException(Error.Format(SRResources.InvalidDollarId, queryString.Get("$id")));
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
