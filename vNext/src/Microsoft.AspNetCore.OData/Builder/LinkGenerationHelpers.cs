// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Builder.Conventions;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNetCore.OData.Routing.ODataPath;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Contains helper methods for generating OData links that follow OData URL conventions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LinkGenerationHelpers
    {
        /// <summary>
        /// Generates a self link following the OData URL conventions for the entity represented by <paramref name="entityContext"/>.
        /// </summary>
        /// <param name="resourceContext">The <see cref="ResourceContext"/> representing the entity for which the self link needs to be generated.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The self link following the OData URL conventions.</returns>
        public static Uri GenerateSelfLink(this ResourceContext resourceContext, bool includeCast)
        {
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }

            if (resourceContext.Url == null)
            {
                throw Error.Argument("resourceContext", SRResources.UrlHelperNull, typeof(ResourceContext).Name);
            }

            IList<ODataPathSegment> idLinkPathSegments = resourceContext.GenerateBaseODataPathSegments();

            bool isSameType = resourceContext.StructuredType == resourceContext.NavigationSource.EntityType();
            if (includeCast && !isSameType)
            {
                idLinkPathSegments.Add(new TypeSegment(resourceContext.StructuredType, navigationSource: null));
            }

            string idLink = resourceContext.Url.CreateODataLink(idLinkPathSegments);
            if (idLink == null)
            {
                return null;
            }

            return new Uri(idLink);
        }

        /// <summary>
        /// Generates a navigation link following the OData URL conventions for the entity represented by <paramref name="resourceContext"/> and the given 
        /// navigation property.
        /// </summary>
        /// <param name="resourceContext">The <see cref="ResourceContext"/> representing the entity for which the navigation link needs to be generated.</param>
        /// <param name="navigationProperty">The EDM navigation property.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The navigation link following the OData URL conventions.</returns>
        public static Uri GenerateNavigationPropertyLink(this ResourceContext resourceContext,
            IEdmNavigationProperty navigationProperty, bool includeCast)
        {
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }
            if (resourceContext.Url == null)
            {
                throw Error.Argument("resourceContext", SRResources.UrlHelperNull, typeof(ResourceContext).Name);
            }

            IList<ODataPathSegment> navigationPathSegments = resourceContext.GenerateBaseODataPathSegments();

            if (includeCast)
            {
                navigationPathSegments.Add(new TypeSegment(resourceContext.StructuredType, navigationSource: null));
            }

            navigationPathSegments.Add(new NavigationPropertySegment(navigationProperty, navigationSource: null));

            string link = resourceContext.Url.CreateODataLink(navigationPathSegments);
            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }

        /// <summary>
        /// Generates an action link following the OData URL conventions for the action <paramref name="action"/> and bound to the
        /// collection of entity represented by <paramref name="resourceSetContext"/>.
        /// </summary>
        /// <param name="resourceSetContext">The <see cref="ResourceSetContext"/> representing the feed for which the action link needs to be generated.</param>
        /// <param name="action">The action for which the action link needs to be generated.</param>
        /// <returns>The generated action link following OData URL conventions.</returns>
        public static Uri GenerateActionLink(this ResourceSetContext resourceSetContext, IEdmOperation action)
        {
            if (resourceSetContext == null)
            {
                throw Error.ArgumentNull("resourceSetContext");
            }

            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            IEdmOperationParameter bindingParameter = action.Parameters.FirstOrDefault();
            if (bindingParameter == null ||
                !bindingParameter.Type.IsCollection() ||
                !((IEdmCollectionType)bindingParameter.Type.Definition).ElementType.IsEntity())
            {
                throw Error.Argument("action", "TODO: "/*SRResources.ActionNotBoundToCollectionOfEntity, action.Name*/);
            }

            return GenerateActionLink(resourceSetContext, bindingParameter.Type, action);
        }

        internal static Uri GenerateActionLink(this ResourceSetContext feedContext, string bindingParameterType,
            string actionName)
        {
            Contract.Assert(feedContext != null);

            if (feedContext.EntitySetBase is IEdmContainedEntitySet)
            {
                return null;
            }

            if (feedContext.EdmModel == null)
            {
                return null;
            }

            IEdmModel model = feedContext.EdmModel;
            string elementType = DeserializationHelpers.GetCollectionElementTypeName(bindingParameterType,
                isNested: false);
            Contract.Assert(elementType != null);

            IEdmTypeReference typeReference = model.FindDeclaredType(elementType).ToEdmTypeReference(true);
            IEdmTypeReference collection = new EdmCollectionTypeReference(new EdmCollectionType(typeReference));

            IEdmOperation operation = model.FindDeclaredOperations(actionName).First();
            return feedContext.GenerateActionLink(collection, operation);
        }

        internal static Uri GenerateActionLink(this ResourceSetContext resourceSetContext, IEdmTypeReference bindingParameterType,
            IEdmOperation action)
        {
            Contract.Assert(resourceSetContext != null);

            if (resourceSetContext.EntitySetBase is IEdmContainedEntitySet)
            {
                return null;
            }

            IList<ODataPathSegment> actionPathSegments = new List<ODataPathSegment>();
            resourceSetContext.GenerateBaseODataPathSegmentsForFeed(actionPathSegments);

            // generate link with cast if the navigation source doesn't match the type the action is bound to.
            if (resourceSetContext.EntitySetBase.Type.FullTypeName() != bindingParameterType.FullName())
            {
                actionPathSegments.Add(new TypeSegment(bindingParameterType.Definition, resourceSetContext.EntitySetBase));
            }

            OperationSegment operationSegment = new OperationSegment(action, entitySet: null);
            actionPathSegments.Add(operationSegment);

            string actionLink = resourceSetContext.Url.CreateODataLink(actionPathSegments);
            return actionLink == null ? null : new Uri(actionLink);
        }

        /// <summary>
        /// Generates a function link following the OData URL conventions for the function <paramref name="function"/> and bound to the
        /// collection of entity represented by <paramref name="resourceSetContext"/>.
        /// </summary>
        /// <param name="resourceSetContext">The <see cref="ResourceSetContext"/> representing the feed for which the function link needs to be generated.</param>
        /// <param name="function">The function for which the function link needs to be generated.</param>
        /// <returns>The generated function link following OData URL conventions.</returns>
        public static Uri GenerateFunctionLink(this ResourceSetContext resourceSetContext, IEdmOperation function)
        {
            if (resourceSetContext == null)
            {
                throw Error.ArgumentNull("resourceSetContext");
            }

            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            IEdmOperationParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null ||
                !bindingParameter.Type.IsCollection() ||
                !((IEdmCollectionType)bindingParameter.Type.Definition).ElementType.IsEntity())
            {
                throw Error.Argument("function", "TODO: "/*SRResources.FunctionNotBoundToCollectionOfEntity, function.Name*/);
            }

            return GenerateFunctionLink(resourceSetContext, bindingParameter.Type, function,
                function.Parameters.Select(p => p.Name));
        }

        internal static Uri GenerateFunctionLink(this ResourceSetContext resourceSetContext, IEdmTypeReference bindingParameterType,
            IEdmOperation functionImport, IEnumerable<string> parameterNames)
        {
            Contract.Assert(resourceSetContext != null);

            if (resourceSetContext.EntitySetBase is IEdmContainedEntitySet)
            {
                return null;
            }

            IList<ODataPathSegment> functionPathSegments = new List<ODataPathSegment>();
            resourceSetContext.GenerateBaseODataPathSegmentsForFeed(functionPathSegments);

            // generate link with cast if the navigation source type doesn't match the entity type the function is bound to.
            if (resourceSetContext.EntitySetBase.Type.FullTypeName() != bindingParameterType.Definition.FullTypeName())
            {
                functionPathSegments.Add(new TypeSegment(bindingParameterType.Definition, null));
            }

            IList<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>();
            // skip the binding parameter
            foreach (string param in parameterNames.Skip(1))
            {
                string value = "@" + param;
                parameters.Add(new OperationSegmentParameter(param, new ConstantNode(value, value)));
            }

            OperationSegment segment = new OperationSegment(new[] { functionImport }, parameters, null);
            functionPathSegments.Add(segment);

            string functionLink = resourceSetContext.Url.CreateODataLink(functionPathSegments);
            return functionLink == null ? null : new Uri(functionLink);
        }

        internal static Uri GenerateFunctionLink(this ResourceSetContext feedContext, string bindingParameterType,
            string functionName, IEnumerable<string> parameterNames)
        {
            Contract.Assert(feedContext != null);

            if (feedContext.EntitySetBase is IEdmContainedEntitySet)
            {
                return null;
            }

            if (feedContext.EdmModel == null)
            {
                return null;
            }

            IEdmModel model = feedContext.EdmModel;

            string elementType = DeserializationHelpers.GetCollectionElementTypeName(bindingParameterType,
                isNested: false);
            Contract.Assert(elementType != null);

            IEdmTypeReference typeReference = model.FindDeclaredType(elementType).ToEdmTypeReference(true);
            IEdmTypeReference collection = new EdmCollectionTypeReference(new EdmCollectionType(typeReference));
            IEdmOperation operation = model.FindDeclaredOperations(functionName).First();
            return feedContext.GenerateFunctionLink(collection, operation, parameterNames);
        }

        /// <summary>
        /// Generates an action link following the OData URL conventions for the action <paramref name="action"/> and bound to the entity
        /// represented by <paramref name="resourceContext"/>.
        /// </summary>
        /// <param name="resourceContext">The <see cref="ResourceContext"/> representing the entity for which the action link needs to be generated.</param>
        /// <param name="action">The action for which the action link needs to be generated.</param>
        /// <returns>The generated action link following OData URL conventions.</returns>
        public static Uri GenerateActionLink(this ResourceContext resourceContext, IEdmOperation action)
        {
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            IEdmOperationParameter bindingParameter = action.Parameters.FirstOrDefault();
            if (bindingParameter == null || !bindingParameter.Type.IsEntity())
            {
                throw Error.Argument("action", SRResources.ActionNotBoundToEntity, action.Name);
            }

            return GenerateActionLink(resourceContext, bindingParameter.Type, action);
        }

        internal static Uri GenerateActionLink(this ResourceContext resourceContext,
            IEdmTypeReference bindingParameterType, IEdmOperation action)
        {
            Contract.Assert(resourceContext != null);
            if (resourceContext.NavigationSource is IEdmContainedEntitySet)
            {
                return null;
            }

            IList<ODataPathSegment> actionPathSegments = resourceContext.GenerateBaseODataPathSegments();

            // generate link with cast if the navigation source doesn't match the entity type the action is bound to.
            if (resourceContext.NavigationSource.EntityType() != bindingParameterType.Definition)
            {
                actionPathSegments.Add(new TypeSegment((IEdmEntityType)bindingParameterType.Definition, null));
                    // entity set can be null
            }

            OperationSegment operationSegment = new OperationSegment(new[] { action }, null);
            actionPathSegments.Add(operationSegment);

            string actionLink = resourceContext.Url.CreateODataLink(actionPathSegments);
            return actionLink == null ? null : new Uri(actionLink);
        }

        internal static Uri GenerateActionLink(this ResourceContext resourceContext, string bindingParameterType,
            string actionName)
        {
            Contract.Assert(resourceContext != null);
            if (resourceContext.NavigationSource is IEdmContainedEntitySet)
            {
                return null;
            }

            if (resourceContext.EdmModel == null)
            {
                return null;
            }

            IEdmModel model = resourceContext.EdmModel;
            IEdmTypeReference typeReference = model.FindDeclaredType(bindingParameterType).ToEdmTypeReference(true);
            IEdmOperation operation = model.FindDeclaredOperations(actionName).First();
            return resourceContext.GenerateActionLink(typeReference, operation);
        }

        /// <summary>
        /// Generates an function link following the OData URL conventions for the function <paramref name="function"/> and bound to the entity
        /// represented by <paramref name="resourceContext"/>.
        /// </summary>
        /// <param name="resourceContext">The <see cref="ResourceContext"/> representing the entity for which the function link needs to be generated.</param>
        /// <param name="function">The function for which the function link needs to be generated.</param>
        /// <returns>The generated function link following OData URL conventions.</returns>
        public static Uri GenerateFunctionLink(this ResourceContext resourceContext, IEdmOperation function)
        {
            if (resourceContext == null)
            {
                throw Error.ArgumentNull("resourceContext");
            }
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            IEdmOperationParameter bindingParameter = function.Parameters.FirstOrDefault();
            if (bindingParameter == null || !bindingParameter.Type.IsEntity())
            {
                throw Error.Argument("function", SRResources.FunctionNotBoundToEntity, function.Name);
            }

            return GenerateFunctionLink(resourceContext, bindingParameter.Type.FullName(), function.FullName(),
                function.Parameters.Select(p => p.Name));
        }

        internal static Uri GenerateFunctionLink(this ResourceContext resourceContext,
            IEdmTypeReference bindingParameterType, IEdmOperation function,
            IEnumerable<string> parameterNames)
        {
            IList<ODataPathSegment> functionPathSegments = resourceContext.GenerateBaseODataPathSegments();

            // generate link with cast if the navigation source type doesn't match the entity type the function is bound to.
            if (resourceContext.NavigationSource.EntityType() != bindingParameterType.Definition)
            {
                functionPathSegments.Add(new TypeSegment(bindingParameterType.Definition, null));
            }

            IList<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>();
            // skip the binding parameter
            foreach (string param in parameterNames.Skip(1))
            {
                string value = "@" + param;
                parameters.Add(new OperationSegmentParameter(param, new ConstantNode(value, value)));
            }

            OperationSegment segment = new OperationSegment(new[] { function }, parameters, null);
            functionPathSegments.Add(segment);

            string functionLink = resourceContext.Url.CreateODataLink(functionPathSegments);
            return functionLink == null ? null : new Uri(functionLink);
        }

        internal static Uri GenerateFunctionLink(this ResourceContext resourceContext, string bindingParameterType,
            string functionName, IEnumerable<string> parameterNames)
        {
            Contract.Assert(resourceContext.EdmModel != null);

            if (resourceContext.EdmModel == null)
            {
                return null;
            }

            IEdmModel model = resourceContext.EdmModel;
            IEdmTypeReference typeReference = model.FindDeclaredType(bindingParameterType).ToEdmTypeReference(true);
            IEdmOperation operation = model.FindDeclaredOperations(functionName).First();
            return resourceContext.GenerateFunctionLink(typeReference, operation, parameterNames);
        }

        internal static IList<ODataPathSegment> GenerateBaseODataPathSegments(this ResourceContext resourceContext)
        {
            IList<ODataPathSegment> odataPath = new List<ODataPathSegment>();

            if (resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.Singleton)
            {
                // Per the OData V4 specification, a singleton is expected to be a child of the entity container, and
                // as a result we can make the assumption that it is the only segment in the generated path.
                odataPath.Add(new SingletonSegment((IEdmSingleton)resourceContext.NavigationSource));
            }
            else
            {
                resourceContext.GenerateBaseODataPathSegmentsForEntity(odataPath);
            }

            return odataPath;
        }

        private static void GenerateBaseODataPathSegmentsForNonSingletons(
            ODataPath path,
            IEdmNavigationSource navigationSource,
            IList<ODataPathSegment> odataPath)
        {
            // If the navigation is not a singleton we need to walk all of the path segments to generate a
            // contextually accurate URI.
            bool segmentFound = false;
            bool containedFound = false;
            if (path != null)
            {
                var segments = path.Segments;
                int length = segments.Count;
                int previousNavigationPathIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    ODataPathSegment pathSegment = segments[i];
                    IEdmNavigationSource currentNavigationSource = null;

                    var entitySetPathSegment = pathSegment as EntitySetSegment;
                    if (entitySetPathSegment != null)
                    {
                        currentNavigationSource = entitySetPathSegment.EntitySet;
                    }

                    var navigationPathSegment = pathSegment as NavigationPropertySegment;
                    if (navigationPathSegment != null)
                    {
                        currentNavigationSource = navigationPathSegment.NavigationSource;
                    }
                    if (containedFound)
                    {
                        odataPath.Add(pathSegment);
                    }
                    else
                    {
                        if (navigationPathSegment != null &&
                            navigationPathSegment.NavigationProperty.ContainsTarget)
                        {
                            containedFound = true;
                            //The path should have the last non-contained navigation property
                            if (previousNavigationPathIndex != -1)
                            {
                                for (int j = previousNavigationPathIndex; j <= i; j++)
                                {
                                    odataPath.Add(segments[j]);
                                }
                            }
                        }
                    }

                    // If we've found our target navigation in the path that means we've correctly populated the
                    // segments up to the navigation and we can ignore the remaining segments.
                    if (currentNavigationSource != null)
                    {
                        previousNavigationPathIndex = i;
                        if (currentNavigationSource == navigationSource)
                        {
                            segmentFound = true;
                            break;
                        }
                    }
                }
            }

            if (!segmentFound || !containedFound)
            {
                // If the target navigation was not found in the current path that means we lack any context that
                // would suggest a scenario other than directly accessing an entity set, so we must assume that's
                // the case.
                odataPath.Clear();

                IEdmContainedEntitySet containmnent = navigationSource as IEdmContainedEntitySet;
                if (containmnent != null)
                {
                    EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
                    IEdmEntitySet entitySet = new EdmEntitySet(container, navigationSource.Name,
                        navigationSource.EntityType());
                    odataPath.Add(new EntitySetSegment(entitySet));
                }
                else
                {
                    odataPath.Add(new EntitySetSegment((IEdmEntitySet)navigationSource));
                }
            }
        }

        private static void GenerateBaseODataPathSegmentsForEntity(
            this ResourceContext resourceContext,
            IList<ODataPathSegment> odataPath)
        {
            // If the navigation is not a singleton we need to walk all of the path segments to generate a
            // contextually accurate URI.
            GenerateBaseODataPathSegmentsForNonSingletons(
                resourceContext.SerializerContext.Path, resourceContext.NavigationSource, odataPath);

            odataPath.Add(new KeySegment(ConventionsHelpers.GetEntityKey(resourceContext), resourceContext.StructuredType as IEdmEntityType,
                null));
        }

        private static void GenerateBaseODataPathSegmentsForFeed(
            this ResourceSetContext feedContext,
            IList<ODataPathSegment> odataPath)
        {
            GenerateBaseODataPathSegmentsForNonSingletons(feedContext.Request.ODataFeature().Path,
                feedContext.EntitySetBase,
                odataPath);
        }
    }
}
