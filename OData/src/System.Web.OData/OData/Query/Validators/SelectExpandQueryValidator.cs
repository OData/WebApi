// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Validators
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SelectExpandQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SelectExpandQueryValidator
    {
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

            IEdmModel model = selectExpandQueryOption.Context.Model;
            ValidateRestrictions(selectExpandQueryOption.SelectExpandClause, model);

            if (validationSettings.MaxExpansionDepth > 0)
            {
                if (selectExpandQueryOption.LevelsMaxLiteralExpansionDepth > validationSettings.MaxExpansionDepth)
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

        private static void ValidateRestrictions(SelectExpandClause selectExpandClause, IEdmModel edmModel)
        {
            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                ExpandedNavigationSelectItem expandItem = selectItem as ExpandedNavigationSelectItem;
                if (expandItem != null)
                {
                    NavigationPropertySegment navigationSegment = (NavigationPropertySegment)expandItem.PathToNavigationProperty.LastSegment;
                    IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                    if (EdmLibHelpers.IsNotExpandable(navigationProperty, edmModel))
                    {
                        throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand, navigationProperty.Name));
                    }
                    ValidateRestrictions(expandItem.SelectAndExpand, edmModel);
                }

                PathSelectItem pathSelectItem = selectItem as PathSelectItem;
                if (pathSelectItem != null)
                {
                    ODataPathSegment segment = pathSelectItem.SelectedPath.LastSegment;
                    NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
                    if (navigationPropertySegment != null)
                    {
                        IEdmNavigationProperty navigationProperty = navigationPropertySegment.NavigationProperty;
                        if (EdmLibHelpers.IsNotNavigable(navigationProperty, edmModel))
                        {
                            throw new ODataException(Error.Format(SRResources.NotNavigablePropertyUsedInNavigation, navigationProperty.Name));
                        }
                    }
                }
            }
        }
    }
}
