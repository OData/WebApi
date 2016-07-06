// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SelectExpandQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SelectExpandQueryValidator
    {
        private readonly DefaultQuerySettings _defaultQuerySettings;
        private readonly FilterQueryValidator _filterQueryValidator;
        private SelectExpandQueryOption _selectExpandQueryOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandQueryValidator" /> class.
        /// </summary>>
        public SelectExpandQueryValidator()
            : this(new DefaultQuerySettings())
        {
        }

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
            IEdmModel edmModel, OrderByClause orderByClause)
        {
            if (orderByClause != null)
            {
                SingleValuePropertyAccessNode node = orderByClause.Expression as SingleValuePropertyAccessNode;
                if (node != null &&
                    EdmLibHelpers.IsNotSortable(node.Property, property, structuredType, edmModel,
                        _defaultQuerySettings.EnableOrderBy))
                {
                    throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy,
                        node.Property.Name));
                }
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

        private static void ValidateSelectItem(SelectItem selectItem, IEdmModel edmModel)
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
                    (expandConfiguration != null && expandConfiguration.ExpandType == ExpandType.Disabled))
                {
                    throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand,
                        property.Name));
                }
            }
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
                        ValidateTopInExpand(property, property.ToEntityType(), edmModel, expandItem.TopOption);
                        ValidateCountInExpand(property, property.ToEntityType(), edmModel, expandItem.CountOption);
                        ValidateOrderByInExpand(property, property.ToEntityType(), edmModel, expandItem.OrderByOption);
                        ValidateFilterInExpand(property, property.ToEntityType(), edmModel, expandItem.FilterOption, validationSettings);

                        bool isExpandable;
                        ExpandConfiguration expandConfiguration;
                        if (navigationProperty == null)
                        {
                            IEdmProperty pathProperty = null;
                            IEdmStructuredType pathStructuredType =
                                _selectExpandQueryOption.Context.ElementType as IEdmStructuredType;
                            if (_selectExpandQueryOption.Context.Path != null)
                            {
                                string name;
                                EdmLibHelpers.GetPropertyAndStructuredTypeFromPath(
                                    _selectExpandQueryOption.Context.Path.Segments,
                                    out pathProperty,
                                    out pathStructuredType,
                                    out name);
                            }

                            isExpandable = EdmLibHelpers.IsExpandable(property.Name,
                                pathProperty,
                                pathStructuredType,
                                edmModel,
                                out expandConfiguration);
                            if (isExpandable && expandConfiguration.MaxDepth > 0)
                            {
                                remainDepth = expandConfiguration.MaxDepth;
                            }
                        }
                        else
                        {
                            isExpandable = EdmLibHelpers.IsExpandable(property.Name,
                                navigationProperty,
                                navigationProperty.ToEntityType(),
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
                        }

                        if (!isExpandable)
                        {
                            if (!_defaultQuerySettings.EnableExpand ||
                                (expandConfiguration != null && expandConfiguration.ExpandType == ExpandType.Disabled))
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

                ValidateSelectItem(selectItem, edmModel);
            }
        }
    }
}
