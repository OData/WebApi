// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Builder
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
        public static string GenerateSelfLink(this EntityInstanceContext entityContext, bool includeCast)
        {
            if (entityContext == null)
            {
                throw Error.ArgumentNull("entityContext");
            }
            if (entityContext.Url == null)
            {
                throw Error.Argument("entityContext", SRResources.UrlHelperNull, typeof(EntityInstanceContext).Name);
            }

            List<ODataPathSegment> idLinkPathSegments = new List<ODataPathSegment>();

            idLinkPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            idLinkPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            if (includeCast)
            {
                idLinkPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            string idLink = entityContext.Url.ODataLink(idLinkPathSegments);
            if (idLink == null)
            {
                return null;
            }

            return idLink;
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

            List<ODataPathSegment> navigationPathSegments = new List<ODataPathSegment>();
            navigationPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            navigationPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            if (includeCast)
            {
                navigationPathSegments.Add(new CastPathSegment(entityContext.EntityType));
            }

            navigationPathSegments.Add(new NavigationPathSegment(navigationProperty));

            string link = entityContext.Url.ODataLink(navigationPathSegments);
            if (link == null)
            {
                return null;
            }

            return new Uri(link);
        }

        /// <summary>
        /// Generates a feed self link following the OData URL conventions for the feed represented by <paramref name="feedContext"/>.
        /// </summary>
        /// <param name="feedContext">The <see cref="FeedContext"/> representing the feed for which the self link needs to be generated.</param>
        /// <returns>The generated feed self link following the OData URL conventions.</returns>
        public static Uri GenerateFeedSelfLink(this FeedContext feedContext)
        {
            if (feedContext == null)
            {
                throw Error.ArgumentNull("feedContext");
            }

            string selfLink = feedContext.Url.ODataLink(new EntitySetPathSegment(feedContext.EntitySet));
            return selfLink == null ? null : new Uri(selfLink);
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
            List<ODataPathSegment> actionPathSegments = new List<ODataPathSegment>();
            actionPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            actionPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            // generate link with cast if the entityset type doesn't match the entity type the action is bound to.
            if (entityContext.EntitySet.ElementType.FullName() != bindingParameterType)
            {
                actionPathSegments.Add(new CastPathSegment(bindingParameterType));
            }

            actionPathSegments.Add(new ActionPathSegment(actionName));

            string actionLink = entityContext.Url.ODataLink(actionPathSegments);
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
            List<ODataPathSegment> functionPathSegments = new List<ODataPathSegment>();
            functionPathSegments.Add(new EntitySetPathSegment(entityContext.EntitySet));
            functionPathSegments.Add(new KeyValuePathSegment(ConventionsHelpers.GetEntityKeyValue(entityContext)));

            // generate link with cast if the entityset type doesn't match the entity type the function is bound to.
            if (entityContext.EntitySet.ElementType.FullName() != bindingParameterType)
            {
                functionPathSegments.Add(new CastPathSegment(bindingParameterType));
            }

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();
            foreach (string param in parameterNames)
            {
                parametersDictionary.Add(param, "@" + param);
            }
            functionPathSegments.Add(new FunctionPathSegment(functionName, parametersDictionary));

            string functionLink = entityContext.Url.ODataLink(functionPathSegments);
            return functionLink == null ? null : new Uri(functionLink);
        }
    }
}
