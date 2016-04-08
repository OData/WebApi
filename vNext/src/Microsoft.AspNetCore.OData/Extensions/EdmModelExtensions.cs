using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public static class EdmModelExtensions
	{
		public static bool HasProperty<T, TPropertyType>(this IEdmModel model,
			Expression<Func<T, TPropertyType>> propertyExpression)
		{
			return model.HasProperty(PropertySelectorVisitor.GetSelectedProperty(propertyExpression));
		}

		public static IEdmProperty GetProperty<T, TPropertyType>(this IEdmModel model,
			Expression<Func<T, TPropertyType>> propertyExpression)
		{
			return model.GetProperty(PropertySelectorVisitor.GetSelectedProperty(propertyExpression));
		}

		public static bool HasProperty(this IEdmModel model, PropertyInfo propertyInfo)
		{
			return model.GetProperty(propertyInfo) != null;
		}

		public static bool HasProperty(this IEdmModel model, Type type, string propertyName)
		{
			return model.GetProperty(type, propertyName) != null;
;		}

		public static IEdmProperty GetProperty(this IEdmModel model, Type type, string propertyName)
		{
			var entityType = model.GetEdmType(type) as EdmEntityType;
			var edmProperty = entityType?.Properties().SingleOrDefault(p =>
					p.Name == propertyName
				);
			return edmProperty;
		}

		public static IEdmProperty GetProperty(this IEdmModel model, PropertyInfo propertyInfo)
		{
			var entityType = model.GetEdmType(propertyInfo.DeclaringType) as EdmEntityType;
			var edmProperty = entityType?.Properties().SingleOrDefault(p =>
					p.Name == propertyInfo.Name &&
					p.Type.Definition.IsEquivalentTo(model.GetEdmType(propertyInfo.PropertyType))
				);
			return edmProperty;
		}

		public static IEdmType GetEdmType(this IEdmModel model, Type clrType)
		{
			if (model == null)
			{
				throw Error.ArgumentNull("model");
			}

			return model.FindDeclaredType(clrType.EdmFullName());
		}

		/// <summary>
		///     Gets the <see cref="ActionLinkBuilder" /> to be used while generating action links for the given action.
		/// </summary>
		/// <param name="model">The <see cref="IEdmModel" /> containing the action.</param>
		/// <param name="action">The action for which the link builder is needed.</param>
		/// <returns>
		///     The <see cref="ActionLinkBuilder" /> for the given action if one is set; otherwise, a new
		///     <see cref="ActionLinkBuilder" /> that generates action links following OData URL conventions.
		/// </returns>
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

			var actionLinkBuilder = model.GetAnnotationValue<ActionLinkBuilder>(action);
			if (actionLinkBuilder == null)
			{
				actionLinkBuilder = new ActionLinkBuilder(
					entityInstanceContext => entityInstanceContext.GenerateActionLink(action), true);
				model.SetActionLinkBuilder(action, actionLinkBuilder);
			}

			return actionLinkBuilder;
		}

		/// <summary>
		///     Sets the <see cref="ActionLinkBuilder" /> to be used for generating the OData action link for the given action.
		/// </summary>
		/// <param name="model">The <see cref="IEdmModel" /> containing the entity set.</param>
		/// <param name="action">The action for which the action link is to be generated.</param>
		/// <param name="actionLinkBuilder">The <see cref="ActionLinkBuilder" /> to set.</param>
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
		///     Sets the <see cref="FunctionLinkBuilder" /> to be used for generating the OData function link for the given
		///     function.
		/// </summary>
		/// <param name="model">The <see cref="IEdmModel" /> containing the entity set.</param>
		/// <param name="function">The function for which the function link is to be generated.</param>
		/// <param name="functionLinkBuilder">The <see cref="FunctionLinkBuilder" /> to set.</param>
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
		///     Gets the <see cref="NavigationSourceLinkBuilderAnnotation" /> to be used while generating self and navigation
		///     links for the given navigation source.
		/// </summary>
		/// <param name="model">The <see cref="IEdmModel" /> containing the navigation source.</param>
		/// <param name="navigationSource">The navigation source.</param>
		/// <returns>
		///     The <see cref="NavigationSourceLinkBuilderAnnotation" /> if set for the given the singleton; otherwise,
		///     a new <see cref="NavigationSourceLinkBuilderAnnotation" /> that generates URLs that follow OData URL conventions.
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

			var annotation = model
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
		///     Sets the <see cref="NavigationSourceLinkBuilderAnnotation" /> to be used while generating self and navigation
		///     links for the given navigation source.
		/// </summary>
		/// <param name="model">The <see cref="IEdmModel" /> containing the navigation source.</param>
		/// <param name="navigationSource">The navigation source.</param>
		/// <param name="navigationSourceLinkBuilder">The <see cref="NavigationSourceLinkBuilderAnnotation" /> to set.</param>
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

			var typeMappingCache = model.GetAnnotationValue<ClrTypeCache>(model);
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

		internal static void SetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action,
			OperationTitleAnnotation title)
		{
			Contract.Assert(model != null);
			model.SetAnnotationValue(action, title);
		}
	}
}