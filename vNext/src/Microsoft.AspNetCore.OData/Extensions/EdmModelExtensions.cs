// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Formatter;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class EdmModelExtensions
    {
        public static IEdmType GetEdmType(this IEdmModel model, Type clrType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            return model.FindDeclaredType(clrType.EdmFullName());
        }

        /// <summary>
        /// Gets the <see cref="OperationLinkBuilder"/> to be used while generating operation links for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the operation.</param>
        /// <param name="operation">The operation for which the link builder is needed.</param>
        /// <returns>The <see cref="OperationLinkBuilder"/> for the given operation if one is set; otherwise, a new
        /// <see cref="OperationLinkBuilder"/> that generates operation links following OData URL conventions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static OperationLinkBuilder GetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (operation == null)
            {
                throw Error.ArgumentNull("operation");
            }

            OperationLinkBuilder linkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(operation);
            if (linkBuilder == null)
            {
                linkBuilder = GetDefaultOperationLinkBuilder(operation);
                model.SetOperationLinkBuilder(operation, linkBuilder);
            }

            return linkBuilder;
        }

        /// <summary>
        /// Sets the <see cref="OperationLinkBuilder"/> to be used for generating the OData operation link for the given operation.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="operation">The operation for which the operation link is to be generated.</param>
        /// <param name="operationLinkBuilder">The <see cref="OperationLinkBuilder"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static void SetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation, OperationLinkBuilder operationLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(operation, operationLinkBuilder);
        }

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

        internal static OperationTitleAnnotation GetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action)
        {
            Contract.Assert(model != null);
            return model.GetAnnotationValue<OperationTitleAnnotation>(action);
        }

        internal static void SetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action, OperationTitleAnnotation title)
        {
            Contract.Assert(model != null);
            model.SetAnnotationValue(action, title);
        }

        private static OperationLinkBuilder GetDefaultOperationLinkBuilder(IEdmOperation operation)
        {
            OperationLinkBuilder linkBuilder = null;
            if (operation.Parameters != null)
            {
                if (operation.Parameters.First().Type.IsEntity())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
                else if (operation.Parameters.First().Type.IsCollection())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
            }
            return linkBuilder;
        }
    }
}