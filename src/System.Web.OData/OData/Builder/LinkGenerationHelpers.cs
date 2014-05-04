// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder.Conventions;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
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
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the self link needs to be generated.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The self link following the OData URL conventions.</returns>
        public static Uri GenerateSelfLink(this EntityInstanceContext entityContext, bool includeCast)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
            }
            if (entityContext.Url == null)
            {
                throw Error.Argument("entityContext", SRResources.UrlHelperNull, typeof(EntityInstanceContext).Name);
            }

            IList<ODataPathSegment> idLinkPathSegments = entityContext.GenerateBaseODataPathSegments();

            bool isSameType = entityContext.EntityType == entityContext.NavigationSource.EntityType();
            if (includeCast && !isSameType)
            {
                idLinkPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            string idLink = entityContext.Url.CreateODataLink(idLinkPathSegments);
            if (idLink == null)
            {
                return null;
            }

            return new Uri(idLink);
        }

        /// <summary>
        /// Generates a navigation link following the OData URL conventions for the entity represented by <paramref name="entityContext"/> and the given 
        /// navigation property.
        /// </summary>
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the navigation link needs to be generated.</param>
        /// <param name="navigationProperty">The EDM navigation property.</param>
        /// <param name="includeCast">Represents whether the generated link should have a cast segment representing a type cast.</param>
        /// <returns>The navigation link following the OData URL conventions.</returns>
        public static Uri GenerateNavigationPropertyLink(this EntityInstanceContext entityContext, IEdmNavigationProperty navigationProperty, bool includeCast)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
            }
            if (entityContext.Url == null)
            {
                throw Error.Argument("entityContext", SRResources.UrlHelperNull, typeof(EntityInstanceContext).Name);
            }

            IList<ODataPathSegment> navigationPathSegments = entityContext.GenerateBaseODataPathSegments();

            if (includeCast)
            {
                navigationPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            navigationPathSegments.Add(new NavigationPathSegment(navigationProperty));

            string link = entityContext.Url.CreateODataLink(navigationPathSegments);
            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }

        /// <summary>
        /// Generates an action link following the OData URL conventions for the action <paramref name="action"/> and bound to the entity
        /// represented by <paramref name="entityContext"/>.
        /// </summary>
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the action link needs to be generated.</param>
        /// <param name="action">The action for which the action link needs to be generated.</param>
        /// <returns>The generated action link following OData URL conventions.</returns>
        public static Uri GenerateActionLink(this EntityInstanceContext entityContext, IEdmOperation action)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
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

            return GenerateActionLink(entityContext, bindingParameter.Type.FullName(), action.Name);
        }

        internal static Uri GenerateActionLink(this EntityInstanceContext entityContext, string bindingParameterType, string actionName)
        {
            Contract.Assert(entityContext != null);
            if (entityContext.NavigationSource is IEdmContainedEntitySet)
            {
                return null;
            }

            IList<ODataPathSegment> actionPathSegments = entityContext.GenerateBaseODataPathSegments();

            // generate link with cast if the navigation source doesn't match the entity type the action is bound to.
            if (entityContext.NavigationSource.EntityType().FullName() != bindingParameterType)
            {
                actionPathSegments.Add(new CastPathSegment(bindingParameterType));
            }

            actionPathSegments.Add(new BoundActionPathSegment(actionName));

            string actionLink = entityContext.Url.CreateODataLink(actionPathSegments);
            return actionLink == null ? null : new Uri(actionLink);
        }

        /// <summary>
        /// Generates an function link following the OData URL conventions for the function <paramref name="function"/> and bound to the entity
        /// represented by <paramref name="entityContext"/>.
        /// </summary>
        /// <param name="entityContext">The <see cref="EntityInstanceContext"/> representing the entity for which the function link needs to be generated.</param>
        /// <param name="function">The function for which the function link needs to be generated.</param>
        /// <returns>The generated function link following OData URL conventions.</returns>
        public static Uri GenerateFunctionLink(this EntityInstanceContext entityContext, IEdmOperation function)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
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

            return GenerateFunctionLink(entityContext, bindingParameter.Type.FullName(), function.Name, function.Parameters.Select(p => p.Name));
        }

        internal static Uri GenerateFunctionLink(this EntityInstanceContext entityContext, string bindingParameterType, string functionName, IEnumerable<string> parameterNames)
        {
            IList<ODataPathSegment> functionPathSegments = entityContext.GenerateBaseODataPathSegments();

            // generate link with cast if the navigation source type doesn't match the entity type the function is bound to.
            if (entityContext.NavigationSource.EntityType().FullName() != bindingParameterType)
            {
                functionPathSegments.Add(new CastPathSegment(bindingParameterType));
            }

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();
            foreach (string param in parameterNames)
            {
                parametersDictionary.Add(param, "@" + param);
            }

            functionPathSegments.Add(new BoundFunctionPathSegment(functionName, parametersDictionary));

            string functionLink = entityContext.Url.CreateODataLink(functionPathSegments);
            return functionLink == null ? null : new Uri(functionLink);
        }

        internal static IList<ODataPathSegment> GenerateBaseODataPathSegments(this EntityInstanceContext entityContext)
        {
            IList<ODataPathSegment> odataPath = new List<ODataPathSegment>();

            if (entityContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.Singleton)
            {
                odataPath.Add(new SingletonPathSegment((IEdmSingleton)entityContext.NavigationSource));
            }
            else
            {
                odataPath.Add(new EntitySetPathSegment((IEdmEntitySetBase)entityContext.NavigationSource));
                odataPath.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));
            }

            return odataPath;
        }
    }
}
