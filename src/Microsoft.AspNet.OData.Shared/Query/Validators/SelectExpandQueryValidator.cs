//-----------------------------------------------------------------------------
// <copyright file="SelectExpandQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SelectExpandQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SelectExpandQueryValidator
    {
        private readonly DefaultQuerySettings _defaultQuerySettings;
        private readonly FilterQueryValidator _filterQueryValidator;
        private OrderByModelLimitationsValidator _orderByQueryValidator;
        private SelectExpandQueryOption _selectExpandQueryOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandQueryValidator" /> class based on
        /// the <see cref="DefaultQuerySettings" />.
        /// </summary>
        /// <param name="defaultQuerySettings">The <see cref="DefaultQuerySettings" />.</param>
        public SelectExpandQueryValidator(DefaultQuerySettings defaultQuerySettings)
        {
            _defaultQuerySettings = defaultQuerySettings;
            _filterQueryValidator = new FilterQueryValidator(_defaultQuerySettings);
        }

        /// <summary>
        /// Validates a <see cref="TopQueryOption" />.
        /// </summary>
        /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings)
        {
            if (selectExpandQueryOption == null)
            {
                throw Error.ArgumentNull("selectExpandQueryOption");
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            _orderByQueryValidator = new OrderByModelLimitationsValidator(selectExpandQueryOption.Context,
                _defaultQuerySettings.EnableOrderBy);
            _selectExpandQueryOption = selectExpandQueryOption;
            ValidateRestrictions(null, 0, selectExpandQueryOption.SelectExpandClause, null, validationSettings);

            if (validationSettings.MaxExpansionDepth > 0)
            {
                if (selectExpandQueryOption.LevelsMaxLiteralExpansionDepth < 0)
                {
                    selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = validationSettings.MaxExpansionDepth;
                }
                else if (selectExpandQueryOption.LevelsMaxLiteralExpansionDepth > validationSettings.MaxExpansionDepth)
                {
                    throw new ODataException(Error.Format(
                        SRResources.InvalidExpansionDepthValue,
                        "LevelsMaxLiteralExpansionDepth",
                        "MaxExpansionDepth"));
                }

                ValidateDepth(selectExpandQueryOption.SelectExpandClause, validationSettings.MaxExpansionDepth);
            }
        }

        internal static SelectExpandQueryValidator GetSelectExpandQueryValidator(ODataQueryContext context)
        {
            if (context == null)
            {
                return new SelectExpandQueryValidator(new DefaultQuerySettings());
            }

            return context.RequestContainer == null
                ? new SelectExpandQueryValidator(context.DefaultQuerySettings)
                : context.RequestContainer.GetRequiredService<SelectExpandQueryValidator>();
        }

        private static void ValidateDepth(SelectExpandClause selectExpand, int maxDepth)
        {
            // do a DFS to see if there is any node that is too deep.
            Stack<Tuple<int, SelectExpandClause>> nodesToVisit = new Stack<Tuple<int, SelectExpandClause>>();
            nodesToVisit.Push(Tuple.Create(0, selectExpand));
            while (nodesToVisit.Count > 0)
            {
                Tuple<int, SelectExpandClause> tuple = nodesToVisit.Pop();
                int currentDepth = tuple.Item1;
                SelectExpandClause currentNode = tuple.Item2;

                ExpandedNavigationSelectItem[] expandItems = currentNode.SelectedItems.OfType<ExpandedNavigationSelectItem>().ToArray();

                if (expandItems.Length > 0 &&
                    ((currentDepth == maxDepth &&
                    expandItems.Any(expandItem =>
                        expandItem.LevelsOption == null ||
                        expandItem.LevelsOption.IsMaxLevel ||
                        expandItem.LevelsOption.Level != 0)) ||
                    expandItems.Any(expandItem =>
                        expandItem.LevelsOption != null &&
                        !expandItem.LevelsOption.IsMaxLevel &&
                        (expandItem.LevelsOption.Level > Int32.MaxValue ||
                        expandItem.LevelsOption.Level + currentDepth > maxDepth))))
                {
                    throw new ODataException(
                        Error.Format(SRResources.MaxExpandDepthExceeded, maxDepth, "MaxExpansionDepth"));
                }

                foreach (ExpandedNavigationSelectItem expandItem in expandItems)
                {
                    int depth = currentDepth + 1;

                    if (expandItem.LevelsOption != null && !expandItem.LevelsOption.IsMaxLevel)
                    {
                        // Add the value of $levels for next depth.
                        depth = depth + (int)expandItem.LevelsOption.Level - 1;
                    }

                    nodesToVisit.Push(Tuple.Create(depth, expandItem.SelectAndExpand));
                }
            }
        }

        private void ValidateTopInExpand(IEdmProperty property, IEdmStructuredType structuredType,
            IEdmModel edmModel, long? topOption)
        {
            if (topOption != null)
            {
                Contract.Assert(topOption.Value <= Int32.MaxValue);
                int maxTop;
                if (EdmLibHelpers.IsTopLimitExceeded(
                    property,
                    structuredType,
                    edmModel,
                    (int)topOption.Value,
                    _defaultQuerySettings,
                    out maxTop))
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop,
                        AllowedQueryOptions.Top, topOption.Value));
                }
            }
        }

        private void ValidateCountInExpand(IEdmProperty property, IEdmStructuredType structuredType, IEdmModel edmModel,
            bool? countOption)
        {
            if (countOption == true)
            {
                if (EdmLibHelpers.IsNotCountable(
                    property,
                    structuredType,
                    edmModel,
                    _defaultQuerySettings.EnableCount))
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.NotCountablePropertyUsedForCount,
                        property.Name));
                }
            }
        }

        private void ValidateOrderByInExpand(IEdmProperty property, IEdmStructuredType structuredType,
            OrderByClause orderByClause)
        {
            if (orderByClause != null)
            {
                _orderByQueryValidator.TryValidate(property, structuredType, orderByClause, false);
            }
        }

        private void ValidateFilterInExpand(IEdmProperty property, IEdmStructuredType structuredType, IEdmModel edmModel,
            FilterClause filterClause, ODataValidationSettings validationSettings)
        {
            if (filterClause != null)
            {
                _filterQueryValidator.Validate(property, structuredType, filterClause, validationSettings, edmModel);
            }
        }

        private void ValidateSelectItem(SelectItem selectItem, IEdmProperty pathProperty, IEdmStructuredType pathStructuredType,
            IEdmModel edmModel)
        {
            PathSelectItem pathSelectItem = selectItem as PathSelectItem;
            if (pathSelectItem != null)
            {
                ODataPathSegment segment = pathSelectItem.SelectedPath.LastSegment;
                NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
                if (navigationPropertySegment != null)
                {
                    IEdmNavigationProperty property = navigationPropertySegment.NavigationProperty;
                    if (EdmLibHelpers.IsNotNavigable(property, edmModel))
                    {
                        throw new ODataException(Error.Format(SRResources.NotNavigablePropertyUsedInNavigation,
                            property.Name));
                    }
                }
                else
                {
                    PropertySegment propertySegment = segment as PropertySegment;
                    if (propertySegment != null)
                    {
                        if (EdmLibHelpers.IsNotSelectable(propertySegment.Property, pathProperty, pathStructuredType, edmModel,
                            _defaultQuerySettings.EnableSelect))
                        {
                            throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect,
                                propertySegment.Property.Name));
                        }
                    }
                }
            }
            else
            {
                WildcardSelectItem wildCardSelectItem = selectItem as WildcardSelectItem;
                if (wildCardSelectItem != null)
                {
                    foreach (var property in pathStructuredType.StructuralProperties())
                    {
                        if (EdmLibHelpers.IsNotSelectable(property, pathProperty, pathStructuredType, edmModel,
                            _defaultQuerySettings.EnableSelect))
                        {
                            throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect,
                                property.Name));
                        }
                    }
                }
            }
        }

        private void ValidateLevelsOption(LevelsClause levelsClause, int depth, int currentDepth,
            IEdmModel edmModel, IEdmNavigationProperty property)
        {
            ExpandConfiguration expandConfiguration;
            bool isExpandable = EdmLibHelpers.IsExpandable(property.Name,
                property,
                property.ToEntityType(),
                edmModel,
                out expandConfiguration);
            if (isExpandable)
            {
                int maxDepth = expandConfiguration.MaxDepth;
                if (maxDepth > 0 && maxDepth < depth)
                {
                    depth = maxDepth;
                }

                if ((depth == 0 && levelsClause.IsMaxLevel) || (depth < levelsClause.Level))
                {
                    throw new ODataException(
                        Error.Format(SRResources.MaxExpandDepthExceeded, currentDepth + depth, "MaxExpansionDepth"));
                }
            }
            else
            {
                if (!_defaultQuerySettings.EnableExpand ||
                    (expandConfiguration != null && expandConfiguration.ExpandType == SelectExpandType.Disabled))
                {
                    throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand,
                        property.Name));
                }
            }
        }

        private void ValidateOtherQueryOptionInExpand(
            IEdmNavigationProperty property,
            IEdmModel edmModel,
            ExpandedNavigationSelectItem expandItem,
            ODataValidationSettings validationSettings)
        {
            ValidateTopInExpand(property, property.ToEntityType(), edmModel, expandItem.TopOption);
            ValidateCountInExpand(property, property.ToEntityType(), edmModel, expandItem.CountOption);
            ValidateOrderByInExpand(property, property.ToEntityType(), expandItem.OrderByOption);
            ValidateFilterInExpand(property, property.ToEntityType(), edmModel, expandItem.FilterOption, validationSettings);
        }

        private void ValidateRestrictions(
            int? remainDepth,
            int currentDepth,
            SelectExpandClause selectExpandClause,
            IEdmNavigationProperty navigationProperty,
            ODataValidationSettings validationSettings)
        {
            IEdmModel edmModel = _selectExpandQueryOption.Context.Model;
            int? depth = remainDepth;
            if (remainDepth < 0)
            {
                throw new ODataException(
                    Error.Format(SRResources.MaxExpandDepthExceeded, currentDepth - 1, "MaxExpansionDepth"));
            }

            IEdmProperty pathProperty;
            IEdmStructuredType pathStructuredType;

            if (navigationProperty == null)
            {
                pathProperty = _selectExpandQueryOption.Context.TargetProperty;
                pathStructuredType = _selectExpandQueryOption.Context.TargetStructuredType;
            }
            else
            {
                pathProperty = navigationProperty;
                pathStructuredType = navigationProperty.ToEntityType();
            }

            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                ExpandedNavigationSelectItem expandItem = selectItem as ExpandedNavigationSelectItem;
                if (expandItem != null)
                {
                    NavigationPropertySegment navigationSegment =
                        (NavigationPropertySegment)expandItem.PathToNavigationProperty.LastSegment;
                    IEdmNavigationProperty property = navigationSegment.NavigationProperty;
                    if (EdmLibHelpers.IsNotExpandable(property, edmModel))
                    {
                        throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand,
                            property.Name));
                    }

                    if (edmModel != null)
                    {
                        ValidateOtherQueryOptionInExpand(property, edmModel, expandItem, validationSettings);
                        bool isExpandable;
                        ExpandConfiguration expandConfiguration;
                        isExpandable = EdmLibHelpers.IsExpandable(property.Name,
                            pathProperty,
                            pathStructuredType,
                            edmModel,
                            out expandConfiguration);
                        if (isExpandable)
                        {
                            int maxDepth = expandConfiguration.MaxDepth;
                            if (maxDepth > 0 && (remainDepth == null || maxDepth < remainDepth))
                            {
                                remainDepth = maxDepth;
                            }
                        }
                        else if (!isExpandable)
                        {
                            if (!_defaultQuerySettings.EnableExpand ||
                                (expandConfiguration != null && expandConfiguration.ExpandType == SelectExpandType.Disabled))
                            {
                                throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand,
                                    property.Name));
                            }
                        }
                    }

                    if (remainDepth.HasValue)
                    {
                        remainDepth--;
                        if (expandItem.LevelsOption != null)
                        {
                            ValidateLevelsOption(expandItem.LevelsOption, remainDepth.Value, currentDepth + 1, edmModel,
                                property);
                        }
                    }

                    ValidateRestrictions(remainDepth, currentDepth + 1, expandItem.SelectAndExpand, property,
                        validationSettings);
                    remainDepth = depth;
                }

                ValidateSelectItem(selectItem, pathProperty, pathStructuredType, edmModel);
            }
        }
    }
}
