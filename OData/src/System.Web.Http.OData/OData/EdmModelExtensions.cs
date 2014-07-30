// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Provides extension methods for the <see cref="IEdmModel"/> interface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EdmModelExtensions
    {
        /// <summary>
        /// Gets the <see cref="EntitySetLinkBuilderAnnotation"/> to be used while generating self and navigation links for the given entity set.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The <see cref="EntitySetLinkBuilderAnnotation"/> if set for the given the entity set; otherwise, a new 
        /// <see cref="EntitySetLinkBuilderAnnotation"/> that generates URLs that follow OData URL conventions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "IEdmEntitySet is more relevant here.")]
        public static EntitySetLinkBuilderAnnotation GetEntitySetLinkBuilder(this IEdmModel model, IEdmEntitySet entitySet)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            EntitySetLinkBuilderAnnotation annotation = model.GetAnnotationValue<EntitySetLinkBuilderAnnotation>(entitySet);
            if (annotation == null)
            {
                // construct and set an entity set link builder that follows OData URL conventions.
                annotation = new EntitySetLinkBuilderAnnotation(entitySet, model);
                model.SetEntitySetLinkBuilder(entitySet, annotation);
            }

            return annotation;
        }

        /// <summary>
        /// Sets the <see cref="EntitySetLinkBuilderAnnotation"/> to be used while generating self and navigation links for the given entity set.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="entitySet">The entity set.</param>
        /// <param name="entitySetLinkBuilder">The <see cref="EntitySetLinkBuilderAnnotation"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "IEdmEntitySet is more relevant here.")]
        public static void SetEntitySetLinkBuilder(this IEdmModel model, IEdmEntitySet entitySet, EntitySetLinkBuilderAnnotation entitySetLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(entitySet, entitySetLinkBuilder);
        }

        /// <summary>
        /// Gets the <see cref="ActionLinkBuilder"/> to be used while generating action links for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the action.</param>
        /// <param name="action">The action for which the link builder is needed.</param>
        /// <returns>The <see cref="ActionLinkBuilder"/> for the given action if one is set; otherwise, a new <see cref="ActionLinkBuilder"/> that 
        /// generates action links following OData URL conventions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "IEdmFunctionImport is more relevant here.")]
        public static ActionLinkBuilder GetActionLinkBuilder(this IEdmModel model, IEdmFunctionImport action)
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
                actionLinkBuilder = new ActionLinkBuilder(entityInstanceContext => entityInstanceContext.GenerateActionLink(action), followsConventions: true);
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
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "IEdmFunctionImport is more relevant here.")]
        public static void SetActionLinkBuilder(this IEdmModel model, IEdmFunctionImport action, ActionLinkBuilder actionLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(action, actionLinkBuilder);
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
    }
}
