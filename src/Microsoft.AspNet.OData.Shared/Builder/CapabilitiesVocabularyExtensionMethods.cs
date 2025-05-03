//-----------------------------------------------------------------------------
// <copyright file="CapabilitiesVocabularyExtensionMethods.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Contains extension methods for <see cref="EdmModel"/> to set the query capabilities vocabulary.
    /// </summary>
    internal static class CapabilitiesVocabularyExtensionMethods
    {
        private static readonly IEnumerable<IEdmStructuralProperty> EmptyStructuralProperties = Enumerable.Empty<IEdmStructuralProperty>();
        private static readonly IEnumerable<IEdmNavigationProperty> EmptyNavigationProperties = Enumerable.Empty<IEdmNavigationProperty>();

        private static IEdmEnumType _navigationType;

        /// <summary>
        /// Set Org.OData.Capabilities.V1.CountRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isCountable">This entity set can be counted.</param>
        /// <param name="nonCountableProperties">These collection properties do not allow /$count segments.</param>
        /// <param name="nonCountableNavigationProperties">These navigation properties do not allow /$count segments.</param>
        public static void SetCountRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isCountable,
            IEnumerable<IEdmProperty> nonCountableProperties,
            IEnumerable<IEdmNavigationProperty> nonCountableNavigationProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            nonCountableProperties = nonCountableProperties ?? EmptyStructuralProperties;
            nonCountableNavigationProperties = nonCountableNavigationProperties ?? EmptyNavigationProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.CountRestrictionsCountable,
                    new EdmBooleanConstant(isCountable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.CountRestrictionsNonCountableProperties,
                    new EdmCollectionExpression(
                        nonCountableProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.CountRestrictionsNonCountableNavigationProperties,
                    new EdmCollectionExpression(
                        nonCountableNavigationProperties.Select(p => new EdmNavigationPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.CountRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.NavigationRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="navigability">This entity set supports navigability.</param>
        /// <param name="restrictedProperties">These properties have navigation restrictions on.</param>
        public static void SetNavigationRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target,
            CapabilitiesNavigationType navigability,
            IEnumerable<Tuple<IEdmNavigationProperty, CapabilitiesNavigationType>> restrictedProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            IEdmEnumType navigationType = model.GetCapabilitiesNavigationType();
            if (navigationType == null)
            {
                return;
            }

            restrictedProperties = restrictedProperties ?? new Tuple<IEdmNavigationProperty, CapabilitiesNavigationType>[0];

            string type = new EdmEnumTypeReference(navigationType, false).ToStringLiteral((long)navigability);

            IEnumerable<EdmRecordExpression> propertiesExpression = restrictedProperties.Select(p =>
            {
                var name = new EdmEnumTypeReference(navigationType, false).ToStringLiteral((long)p.Item2);
                return new EdmRecordExpression(new IEdmPropertyConstructor[]
                {
                    new EdmPropertyConstructor(
                        CapabilitiesVocabularyConstants.NavigationPropertyRestrictionNavigationProperty,
                        new EdmNavigationPropertyPathExpression(p.Item1.Name)),
                    new EdmPropertyConstructor(CapabilitiesVocabularyConstants.NavigationRestrictionsNavigability,
                        new EdmEnumMemberExpression(navigationType.Members.Single(m => m.Name == name)))
                });
            });

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.NavigationRestrictionsNavigability,
                    new EdmEnumMemberExpression(navigationType.Members.Single(m => m.Name == type))),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.NavigationRestrictionsRestrictedProperties,
                    new EdmCollectionExpression(propertiesExpression))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.NavigationRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.FilterRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isFilterable">This entity set supports the $filter expressions.</param>
        /// <param name="isRequiresFilter">This entity set requires $filter expressions.</param>
        /// <param name="requiredProperties">These properties must be specified in the $filter clause.</param>
        /// <param name="nonFilterableProperties">These properties cannot be used in $filter expressions.</param>
        public static void SetFilterRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isFilterable,
            bool isRequiresFilter, IEnumerable<IEdmProperty> requiredProperties,
            IEnumerable<IEdmProperty> nonFilterableProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            requiredProperties = requiredProperties ?? EmptyStructuralProperties;
            nonFilterableProperties = nonFilterableProperties ?? EmptyStructuralProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.FilterRestrictionsFilterable,
                    new EdmBooleanConstant(isFilterable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.FilterRestrictionsRequiresFilter,
                    new EdmBooleanConstant(isRequiresFilter)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.FilterRestrictionsRequiredProperties,
                    new EdmCollectionExpression(
                        requiredProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.FilterRestrictionsNonFilterableProperties,
                    new EdmCollectionExpression(
                        nonFilterableProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.FilterRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.SortRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isSortable">This entity set supports the $orderby expressions.</param>
        /// <param name="ascendingOnlyProperties">These properties can only be used for sorting in ascending order.</param>
        /// <param name="descendingOnlyProperties">These properties can only be used for sorting in descending order.</param>
        /// <param name="nonSortableProperties">These properties cannot be used in $orderby expressions.</param>
        public static void SetSortRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isSortable,
            IEnumerable<IEdmProperty> ascendingOnlyProperties, IEnumerable<IEdmProperty> descendingOnlyProperties,
            IEnumerable<IEdmProperty> nonSortableProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            ascendingOnlyProperties = ascendingOnlyProperties ?? EmptyStructuralProperties;
            descendingOnlyProperties = descendingOnlyProperties ?? EmptyStructuralProperties;
            nonSortableProperties = nonSortableProperties ?? EmptyStructuralProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.SortRestrictionsSortable,
                    new EdmBooleanConstant(isSortable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.SortRestrictionsAscendingOnlyProperties,
                    new EdmCollectionExpression(
                        ascendingOnlyProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.SortRestrictionsDescendingOnlyProperties,
                    new EdmCollectionExpression(
                        descendingOnlyProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.SortRestrictionsNonSortableProperties,
                    new EdmCollectionExpression(
                        nonSortableProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.SortRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.ExpandRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isExpandable">This entity set supports the expand expressions.</param>
        /// <param name="nonExpandableProperties">These properties cannot be used in $expand expressions.</param>
        public static void SetExpandRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isExpandable,
            IEnumerable<IEdmNavigationProperty> nonExpandableProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            nonExpandableProperties = nonExpandableProperties ?? EmptyNavigationProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.ExpandRestrictionsExpandable,
                    new EdmBooleanConstant(isExpandable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.ExpandRestrictionsNonExpandableProperties,
                    new EdmCollectionExpression(
                        nonExpandableProperties.Select(p => new EdmNavigationPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.ExpandRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.InsertRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isInsertable">This entity set can be inserted.</param>
        /// <param name="nonInsertableProperties">These properties SHOULD be omitted from POST requests because the server will ignore their values.</param>
        /// <param name="nonInsertableNavigationProperties">These navigation properties do not allow InsertLink requests.</param>
        public static void SetInsertRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isInsertable,
            IEnumerable<IEdmProperty> nonInsertableProperties,
            IEnumerable<IEdmNavigationProperty> nonInsertableNavigationProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            nonInsertableProperties = nonInsertableProperties ?? EmptyStructuralProperties;
            nonInsertableNavigationProperties = nonInsertableNavigationProperties ?? EmptyNavigationProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.InsertRestrictionsInsertable,
                    new EdmBooleanConstant(isInsertable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.InsertRestrictionsNonInsertableProperties,
                    new EdmCollectionExpression(
                        nonInsertableProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.InsertRestrictionsNonInsertableNavigationProperties,
                    new EdmCollectionExpression(
                        nonInsertableNavigationProperties.Select(p => new EdmNavigationPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.InsertRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.UpdateRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isUpdatable">This entity set can be updated.</param>
        /// <param name="nonUpdatableProperties">These properties SHOULD be omitted from PUT or PATCH requests because the server will ignore their values.These properties do not allow UpdateValue and DeleteValue requests.</param>
        /// <param name="nonUpdatableNavigationProperties">These navigation properties do not allow UpdateLink requests.</param>
        public static void SetUpdateRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isUpdatable,
            IEnumerable<IEdmProperty> nonUpdatableProperties,
            IEnumerable<IEdmNavigationProperty> nonUpdatableNavigationProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            nonUpdatableProperties = nonUpdatableProperties ?? EmptyStructuralProperties;
            nonUpdatableNavigationProperties = nonUpdatableNavigationProperties ?? EmptyNavigationProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.UpdateRestrictionsUpdatable,
                    new EdmBooleanConstant(isUpdatable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.UpdateRestrictionsNonUpdatableProperties,
                    new EdmCollectionExpression(
                        nonUpdatableProperties.Select(p => new EdmPropertyPathExpression(p.Name)).ToArray())),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.UpdateRestrictionsNonUpdatableNavigationProperties,
                    new EdmCollectionExpression(
                        nonUpdatableNavigationProperties.Select(p => new EdmNavigationPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.UpdateRestrictions);
        }

        /// <summary>
        /// Set Org.OData.Capabilities.V1.DeleteRestrictions to target.
        /// </summary>
        /// <param name="model">The model referenced to.</param>
        /// <param name="target">The target entity set to set the inline annotation.</param>
        /// <param name="isDeletable">This entity set can be deleted.</param>
        /// <param name="nonDeletableNavigationProperties">These navigation properties do not allow DeleteLink requests.</param>
        public static void SetDeleteRestrictionsAnnotation(this EdmModel model, IEdmEntitySet target, bool isDeletable,
            IEnumerable<IEdmNavigationProperty> nonDeletableNavigationProperties)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (target == null)
            {
                throw Error.ArgumentNull("target");
            }

            nonDeletableNavigationProperties = nonDeletableNavigationProperties ?? EmptyNavigationProperties;

            IList<IEdmPropertyConstructor> properties = new List<IEdmPropertyConstructor>
            {
                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.DeleteRestrictionsDeletable,
                    new EdmBooleanConstant(isDeletable)),

                new EdmPropertyConstructor(CapabilitiesVocabularyConstants.DeleteRestrictionsNonDeletableNavigationProperties,
                    new EdmCollectionExpression(
                        nonDeletableNavigationProperties.Select(p => new EdmNavigationPropertyPathExpression(p.Name)).ToArray()))
            };

            model.SetVocabularyAnnotation(target, properties, CapabilitiesVocabularyConstants.DeleteRestrictions);
        }

        private static void SetVocabularyAnnotation(this EdmModel model, IEdmVocabularyAnnotatable target,
            IList<IEdmPropertyConstructor> properties, string qualifiedName)
        {
            Contract.Assert(model != null);
            Contract.Assert(target != null);

            IEdmTerm term = model.FindTerm(qualifiedName);
            if (term != null)
            {
                IEdmRecordExpression record = new EdmRecordExpression(properties);
                EdmVocabularyAnnotation annotation = new EdmVocabularyAnnotation(target, term, record);
                annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
                model.SetVocabularyAnnotation(annotation);
            }
        }

        private static IEdmEnumType GetCapabilitiesNavigationType(this EdmModel model)
        {
            return _navigationType ??
                   (_navigationType = model.FindType(CapabilitiesVocabularyConstants.NavigationType) as IEdmEnumType);
        }
    }
}
