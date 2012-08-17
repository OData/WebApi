// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SyntacticAst;

namespace System.Web.Http.OData.Formatter
{
    internal static class ODataUriHelpers
    {
        // Build the projection tree given a select and expand and the entity set.
        // TODO: Bug 467621: replace this with the ODataUriParser functionality
        public static ODataQueryProjectionNode GetODataQueryProjectionNode(string selectQuery, string expandQuery, IEdmEntitySet entitySet)
        {
            IEnumerable<string> selects = (selectQuery ?? String.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
            IEnumerable<string> expands = (expandQuery ?? String.Empty).Split(new[] { ',', }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            ODataQueryProjectionNode rootProjectionNode = new ODataQueryProjectionNode { Name = String.Empty, NodeType = entitySet.ElementType };

            foreach (var expand in expands)
            {
                ODataQueryProjectionNode currentProjectionNode = rootProjectionNode;

                IEnumerable<string> expandPath = expand.Split('/');
                foreach (var property in expandPath)
                {
                    ODataQueryProjectionNode nextNode = currentProjectionNode.Expands.SingleOrDefault(node => node.Name == property);
                    if (nextNode == null)
                    {
                        nextNode = new ODataQueryProjectionNode { Name = property, NodeType = (currentProjectionNode.NodeType as IEdmEntityType).Properties().SingleOrDefault(p => p.Name == property).Type.Definition };
                        currentProjectionNode.Expands.Add(nextNode);
                    }

                    currentProjectionNode = nextNode;
                }
            }

            foreach (var select in selects)
            {
                ODataQueryProjectionNode currentProjectionNode = rootProjectionNode;

                IEnumerable<string> selectPath = select.Split('/');
                foreach (var property in selectPath)
                {
                    ODataQueryProjectionNode nextNode = currentProjectionNode.Expands.SingleOrDefault(node => node.Name == property);
                    if (nextNode == null)
                    {
                        nextNode = new ODataQueryProjectionNode { Name = property, NodeType = (currentProjectionNode.NodeType as IEdmEntityType).Properties().SingleOrDefault(p => p.Name == property).Type.Definition };
                        currentProjectionNode.Selects.Add(nextNode);
                    }

                    currentProjectionNode = nextNode;
                }
            }

            return rootProjectionNode;
        }

        /// <summary>
        /// Tries to get entity set.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="model">The model.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>
        /// True  if it gets and entitySet or false if it can't
        /// </returns>
        public static bool TryGetEntitySetAndEntityType(Uri uri, IEdmModel model, out IEdmEntitySet entitySet)
        {
            IEdmEntitySet currentEntitySet = null;

            entitySet = null;
            foreach (var segment in uri.Segments)
            {
                var segmentValue = segment.Replace("/", String.Empty);

                if (segmentValue.Length == 0)
                {
                    continue;
                }

                // lopping off key pieces as we don't care
                int i = segment.IndexOf('(');
                if (i > -1)
                {
                    segmentValue = segment.Remove(i);
                }

                var container = model.EntityContainers().First();
                // If there is no entitySet we need to find out which one it is
                if (currentEntitySet == null)
                {
                    var foundEntitySet = container.FindEntitySet(segmentValue);
                    if (foundEntitySet != null)
                    {
                        currentEntitySet = foundEntitySet;
                    }
                    else
                    {
                        // check to see if there the current segment is a service operation
                        var functionImport = container.FunctionImports().SingleOrDefault(fi => fi.Name == segmentValue);
                        if (functionImport != null)
                        {
                            IEdmEntitySet functionEntitySet = null;
                            if (functionImport.TryGetStaticEntitySet(out functionEntitySet))
                            {
                                currentEntitySet = functionEntitySet;
                            }
                        }
                    }
                }
                else
                {
                    var navigationProperty = currentEntitySet.ElementType.NavigationProperties().SingleOrDefault(np => np.Name == segmentValue);
                    if (navigationProperty != null)
                    {
                        currentEntitySet = currentEntitySet.FindNavigationTarget(navigationProperty);
                    }
                    else
                    {
                        // Need to update this a little so it works for Actions/Functions
                        var functionImport = container.FunctionImports().SingleOrDefault(fi => fi.IsBindable == true && fi.Name == segmentValue);
                        if (functionImport != null)
                        {
                            IEdmEntitySet functionEntitySet = null;
                            if (functionImport.TryGetStaticEntitySet(out functionEntitySet))
                            {
                                currentEntitySet = functionEntitySet;
                            }
                        }
                    }
                }
            }

            if (currentEntitySet != null)
            {
                entitySet = currentEntitySet;
                return true;
            }

            entitySet = null;

            return false;
        }

        // TODO: Bug 467617: figure out the story for the operation name on the client side and server side.
        // This is clearly a workaround. We are assuming that the operation name is the last segment in the request uri 
        // which works for most cases and fall back to the type name of the object being written.
        // We should rather use uri parser semantic tree to figure out the operation name from the request url.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "we really dont want to fail here and the underlying code can throw any exception")]
        public static string GetOperationName(Uri requestUri, Uri baseAddress)
        {
            try
            {
                // remove the query part as they are irrelevant here and we dont want to fail parsing them.
                Uri requestUriWithoutQuerypart = new Uri(requestUri.GetLeftPart(UriPartial.Path));
                SyntacticTree syntacticTree = SyntacticTree.ParseUri(requestUriWithoutQuerypart, baseAddress);
                SegmentQueryToken lastSegment = syntacticTree.Path as SegmentQueryToken;
                if (lastSegment != null && !String.IsNullOrEmpty(lastSegment.Name))
                {
                    return lastSegment.Name;
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
