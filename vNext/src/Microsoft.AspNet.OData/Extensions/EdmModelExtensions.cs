using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;

namespace Microsoft.AspNet.OData.Extensions
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

        internal static void SetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action, OperationTitleAnnotation title)
        {
            Contract.Assert(model != null);
            model.SetAnnotationValue(action, title);
        }
    }
}