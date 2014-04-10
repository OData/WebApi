// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Formatter;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Provides extension methods for the <see cref="IEdmModel"/> interface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EdmModelExtensions
    {
        /// <summary>
        /// Gets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The <see cref="NavigationSourceLinkBuilderAnnotation"/> if set for the given the singleton; otherwise,
        /// a new <see cref="NavigationSourceLinkBuilderAnnotation"/> that generates URLs that follow OData URL conventions.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmNavigationSource is more relevant here.")]
        public static NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder(this IEdmModel model,
            IEdmNavigationSource navigationSource)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            NavigationSourceLinkBuilderAnnotation annotation = model
                .GetAnnotationValue<NavigationSourceLinkBuilderAnnotation>(navigationSource);
            if (annotation == null)
            {
                // construct and set a navigation source link builder that follows OData URL conventions.
                annotation = new NavigationSourceLinkBuilderAnnotation(navigationSource, model);
                model.SetNavigationSourceLinkBuilder(navigationSource, annotation);
            }

            return annotation;
        }

        /// <summary>
        /// Sets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="navigationSourceLinkBuilder">The <see cref="NavigationSourceLinkBuilderAnnotation"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmNavigationSource is more relevant here.")]
        public static void SetNavigationSourceLinkBuilder(this IEdmModel model, IEdmNavigationSource navigationSource,
            NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(navigationSource, navigationSourceLinkBuilder);
        }

        /// <summary>
        /// Gets the <see cref="ActionLinkBuilder"/> to be used while generating action links for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the action.</param>
        /// <param name="action">The action for which the link builder is needed.</param>
        /// <returns>The <see cref="ActionLinkBuilder"/> for the given action if one is set; otherwise, a new
        /// <see cref="ActionLinkBuilder"/> that generates action links following OData URL conventions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static ActionLinkBuilder GetActionLinkBuilder(this IEdmModel model, IEdmAction action)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (action == null)
            {
                throw Error.ArgumentNull("action");
            }

            ActionLinkBuilder actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(action);
            if (actionLinkBuilder == null)
            {
                actionLinkBuilder = new ActionLinkBuilder(
                    entityInstanceContext => entityInstanceContext.GenerateActionLink(action), followsConventions: true);
                model.SetActionLinkBuilder(action, actionLinkBuilder);
            }

            return actionLinkBuilder;
        }

        /// <summary>
        /// Sets the <see cref="ActionLinkBuilder"/> to be used for generating the OData action link for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="action">The action for which the action link is to be generated.</param>
        /// <param name="actionLinkBuilder">The <see cref="ActionLinkBuilder"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static void SetActionLinkBuilder(this IEdmModel model, IEdmAction action, ActionLinkBuilder actionLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(action, actionLinkBuilder);
        }

        /// <summary>
        /// Sets the <see cref="FunctionLinkBuilder"/> to be used for generating the OData function link for the given function.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="function">The function for which the function link is to be generated.</param>
        /// <param name="functionLinkBuilder">The <see cref="FunctionLinkBuilder"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmFunctionImport is more relevant here.")]
        public static void SetFunctionLinkBuilder(this IEdmModel model, IEdmFunction function,
            FunctionLinkBuilder functionLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(function, functionLinkBuilder);
        }

        internal static ClrTypeCache GetTypeMappingCache(this IEdmModel model)
        {
            Contract.Assert(model != null);

            ClrTypeCache typeMappingCache = model.GetAnnotationValue<ClrTypeCache>(model);
            if (typeMappingCache == null)
            {
                typeMappingCache = new ClrTypeCache();
                model.SetAnnotationValue(model, typeMappingCache);
            }

            return typeMappingCache;
        }

        internal static void SetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action, OperationTitleAnnotation title)
        {
            Contract.Assert(model != null);
            model.SetAnnotationValue(action, title);
        }

        internal static OperationTitleAnnotation GetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action)
        {
            Contract.Assert(model != null);
            return model.GetAnnotationValue<OperationTitleAnnotation>(action);
        }
    }
}
