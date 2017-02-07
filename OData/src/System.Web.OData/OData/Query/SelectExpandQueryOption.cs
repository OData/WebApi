﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Query.Validators;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPathSegment = Microsoft.OData.UriParser.ODataPathSegment;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents the OData $select and $expand query options.
    /// </summary>
    public class SelectExpandQueryOption
    {
        private SelectExpandClause _selectExpandClause;
        private ODataQueryOptionParser _queryOptionParser;
        // Give _levelsMaxLiteralExpansionDepth a negative value meaning it is uninitialized, and it will be set to:
        // 1. LevelsMaxLiteralExpansionDepth or
        // 2. ODataValidationSettings.MaxExpansionDepth
        private int _levelsMaxLiteralExpansionDepth = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandQueryOption"/> class.
        /// </summary>
        /// <param name="select">The $select query parameter value.</param>
        /// <param name="expand">The $expand query parameter value.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public SelectExpandQueryOption(string select, string expand, ODataQueryContext context,
            ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(select) && String.IsNullOrEmpty(expand))
            {
                throw Error.Argument(SRResources.SelectExpandEmptyOrNull);
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            IEdmEntityType entityType = context.ElementType as IEdmEntityType;
            if (entityType == null)
            {
                throw Error.Argument("context", SRResources.SelectNonEntity, context.ElementType.ToTraceString());
            }

            Context = context;
            RawSelect = select;
            RawExpand = expand;
            Validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(context);
            _queryOptionParser = queryOptionParser;
        }

        internal SelectExpandQueryOption(
            string select,
            string expand,
            ODataQueryContext context,
            SelectExpandClause selectExpandClause)
            : this(select, expand, context)
        {
            _selectExpandClause = selectExpandClause;
        }

        // This constructor is intended for unit testing only.
        internal SelectExpandQueryOption(string select, string expand, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(select) && String.IsNullOrEmpty(expand))
            {
                throw Error.Argument(SRResources.SelectExpandEmptyOrNull);
            }

            IEdmEntityType entityType = context.ElementType as IEdmEntityType;
            if (entityType == null)
            {
                throw Error.Argument("context", SRResources.SelectNonEntity, context.ElementType.ToTraceString());
            }

            Context = context;
            RawSelect = select;
            RawExpand = expand;
            Validator = SelectExpandQueryValidator.GetSelectExpandQueryValidator(context);
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$select", select }, { "$expand", expand } });
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $select value.
        /// </summary>
        public string RawSelect { get; private set; }

        /// <summary>
        /// Gets the raw $expand value.
        /// </summary>
        public string RawExpand { get; private set; }

        /// <summary>
        /// Gets or sets the $select and $expand query validator.
        /// </summary>
        public SelectExpandQueryValidator Validator { get; set; }

        /// <summary>
        /// Gets the parsed <see cref="SelectExpandClause"/> for this query option.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get
            {
                if (_selectExpandClause == null)
                {
                    _selectExpandClause = _queryOptionParser.ParseSelectAndExpand();
                }

                return _selectExpandClause;
            }
        }

        /// <summary>
        /// Gets or sets the number of levels that a top level $expand=NavigationProperty($levels=max)
        /// will be expanded.
        /// This value will decrease by one with each nesting level in the $expand clause.
        /// For example, with a property value 5, the following query $expand=A($expand=B($expand=C($levels=max)))
        /// will be interpreted as $expand=A($expand=B($expand=C($levels=3))).
        /// If the query gets validated, the <see cref="ODataValidationSettings.MaxExpansionDepth"/> value
        /// must be greater than or equal to this value.
        /// </summary>
        public int LevelsMaxLiteralExpansionDepth
        {
            get
            {
                return _levelsMaxLiteralExpansionDepth;
            }
            set
            {
                if (value < 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("LevelsMaxLiteralExpansionDepth", value, 0);
                }

                _levelsMaxLiteralExpansionDepth = value;
            }
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given <see cref="IQueryable"/> using the given
        /// <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="queryable">The original <see cref="IQueryable"/>.</param>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "stopgap. will be used later.")]
        public IQueryable ApplyTo(IQueryable queryable, ODataQuerySettings settings)
        {
            if (queryable == null)
            {
                throw Error.ArgumentNull("queryable");
            }
            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ODataQuerySettings updatedSettings = Context.UpdateQuerySettings(settings, queryable);

            return SelectExpandBinder.Bind(queryable, updatedSettings, this);
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given entity using the given <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public object ApplyTo(object entity, ODataQuerySettings settings)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (settings == null)
            {
                throw Error.ArgumentNull("settings");
            }
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ODataQuerySettings updatedSettings = Context.UpdateQuerySettings(settings, query: null);

            return SelectExpandBinder.Bind(entity, updatedSettings, this);
        }

        /// <summary>
        /// Validate the $select and $expand query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }

        internal SelectExpandClause ProcessLevels()
        {
            bool levelsEncountered;
            bool isMaxLevel;
            ModelBoundQuerySettings querySettings = EdmLibHelpers.GetModelBoundQuerySettings(Context.TargetProperty,
                Context.TargetStructuredType, Context.Model, Context.DefaultQuerySettings);
            return ProcessLevels(SelectExpandClause,
                LevelsMaxLiteralExpansionDepth < 0 ? ODataValidationSettings.DefaultMaxExpansionDepth : LevelsMaxLiteralExpansionDepth,
                querySettings,
                out levelsEncountered,
                out isMaxLevel);
        }

        // Process $levels in SelectExpandClause.
        private SelectExpandClause ProcessLevels(
            SelectExpandClause selectExpandClause,
            int levelsMaxLiteralExpansionDepth,
            ModelBoundQuerySettings querySettings,
            out bool levelsEncountered,
            out bool isMaxLevel)
        {
            levelsEncountered = false;
            isMaxLevel = false;

            if (selectExpandClause == null)
            {
                return null;
            }

            // Process $levels in SelectItems of SelectExpandClause.
            IEnumerable<SelectItem> selectItems = ProcessLevels(
                selectExpandClause.SelectedItems,
                levelsMaxLiteralExpansionDepth,
                querySettings,
                out levelsEncountered,
                out isMaxLevel);

            if (selectItems == null)
            {
                return null;
            }
            else if (levelsEncountered)
            {
                return new SelectExpandClause(selectItems, selectExpandClause.AllSelected);
            }
            else
            {
                // Return the original SelectExpandClause if no $levels is found.
                return selectExpandClause;
            }
        }

        // Process $levels in SelectedItems.
        private IEnumerable<SelectItem> ProcessLevels(
            IEnumerable<SelectItem> selectItems,
            int levelsMaxLiteralExpansionDepth,
            ModelBoundQuerySettings querySettings,
            out bool levelsEncountered,
            out bool isMaxLevel)
        {
            levelsEncountered = false;
            isMaxLevel = false;
            IList<SelectItem> items = new List<SelectItem>();

            foreach (SelectItem selectItem in selectItems)
            {
                ExpandedNavigationSelectItem item = selectItem as ExpandedNavigationSelectItem;

                if (item == null)
                {
                    // There is no $levels in non-ExpandedNavigationSelectItem.
                    items.Add(selectItem);
                }
                else
                {
                    bool levelsEncouteredInExpand;
                    bool isMaxLevelInExpand;
                    // Process $levels in ExpandedNavigationSelectItem.
                    ExpandedNavigationSelectItem expandItem = ProcessLevels(
                        item,
                        levelsMaxLiteralExpansionDepth,
                        querySettings,
                        out levelsEncouteredInExpand,
                        out isMaxLevelInExpand);

                    if (item.LevelsOption != null && item.LevelsOption.Level > 0 && expandItem == null)
                    {
                        // Abandon this attempt if any of the items failed to expand
                        return null;
                    }
                    else if (item.LevelsOption != null)
                    {
                        // The expansion would be volatile if any of the expand item is max level
                        isMaxLevel = isMaxLevel || isMaxLevelInExpand;
                    }

                    levelsEncountered = levelsEncountered || levelsEncouteredInExpand;

                    if (expandItem != null)
                    {
                        items.Add(expandItem);
                    }
                }
            }
            return items;
        }

        private void GetAutoSelectExpandItems(
            IEdmEntityType baseEntityType,
            IEdmModel model,
            IEdmNavigationSource navigationSource,
            bool isAllSelected,
            ModelBoundQuerySettings modelBoundQuerySettings,
            int depth,
            out List<SelectItem> autoSelectItems,
            out List<SelectItem> autoExpandItems)
        {
            autoSelectItems = new List<SelectItem>();
            var autoSelectProperties = EdmLibHelpers.GetAutoSelectProperties(null,
                baseEntityType, model, modelBoundQuerySettings);
            foreach (var autoSelectProperty in autoSelectProperties)
            {
                List<ODataPathSegment> pathSegments = new List<ODataPathSegment>()
                {
                    new PropertySegment(autoSelectProperty)
                };

                PathSelectItem pathSelectItem = new PathSelectItem(
                    new ODataSelectPath(pathSegments));
                autoSelectItems.Add(pathSelectItem);
            }

            autoExpandItems = new List<SelectItem>();
            depth--;
            if (depth < 0)
            {
                return;
            }

            var autoExpandNavigationProperties = EdmLibHelpers.GetAutoExpandNavigationProperties(null, baseEntityType,
                model, !isAllSelected, modelBoundQuerySettings);

            foreach (var navigationProperty in autoExpandNavigationProperties)
            {
                IEdmNavigationSource currentEdmNavigationSource =
                    navigationSource.FindNavigationTarget(navigationProperty);

                if (currentEdmNavigationSource != null)
                {
                    List<ODataPathSegment> pathSegments = new List<ODataPathSegment>()
                    {
                        new NavigationPropertySegment(navigationProperty, currentEdmNavigationSource)
                    };

                    ODataExpandPath expandPath = new ODataExpandPath(pathSegments);
                    SelectExpandClause selectExpandClause = new SelectExpandClause(new List<SelectItem>(),
                        true);
                    ExpandedNavigationSelectItem item = new ExpandedNavigationSelectItem(expandPath,
                        currentEdmNavigationSource, selectExpandClause);
                    modelBoundQuerySettings = EdmLibHelpers.GetModelBoundQuerySettings(navigationProperty,
                        navigationProperty.ToEntityType(), model);
                    List<SelectItem> nestedSelectItems;
                    List<SelectItem> nestedExpandItems;

                    int maxExpandDepth = GetMaxExpandDepth(modelBoundQuerySettings, navigationProperty.Name);
                    if (maxExpandDepth != 0 && maxExpandDepth < depth)
                    {
                        depth = maxExpandDepth;
                    }

                    GetAutoSelectExpandItems(
                        currentEdmNavigationSource.EntityType(),
                        model,
                        item.NavigationSource,
                        true,
                        modelBoundQuerySettings,
                        depth,
                        out nestedSelectItems,
                        out nestedExpandItems);

                    selectExpandClause = new SelectExpandClause(nestedSelectItems.Concat(nestedExpandItems),
                        nestedSelectItems.Count == 0);
                    item = new ExpandedNavigationSelectItem(expandPath, currentEdmNavigationSource,
                        selectExpandClause);

                    autoExpandItems.Add(item);
                    if (!isAllSelected || autoSelectProperties.Count() != 0)
                    {
                        PathSelectItem pathSelectItem = new PathSelectItem(
                            new ODataSelectPath(pathSegments));
                        autoExpandItems.Add(pathSelectItem);
                    }
                }
            }
        }

        // Process $levels in ExpandedNavigationSelectItem.
        private ExpandedNavigationSelectItem ProcessLevels(
            ExpandedNavigationSelectItem expandItem,
            int levelsMaxLiteralExpansionDepth,
            ModelBoundQuerySettings querySettings,
            out bool levelsEncounteredInExpand,
            out bool isMaxLevelInExpand)
        {
            int level;
            isMaxLevelInExpand = false;

            if (expandItem.LevelsOption == null)
            {
                levelsEncounteredInExpand = false;
                level = 1;
            }
            else
            {
                levelsEncounteredInExpand = true;
                if (expandItem.LevelsOption.IsMaxLevel)
                {
                    isMaxLevelInExpand = true;
                    level = levelsMaxLiteralExpansionDepth;
                }
                else
                {
                    level = (int)expandItem.LevelsOption.Level;
                }
            }

            // Do not expand when:
            // 1. $levels is equal to or less than 0.
            // 2. $levels value is greater than current MaxExpansionDepth
            if (level <= 0 || level > levelsMaxLiteralExpansionDepth)
            {
                return null;
            }

            ExpandedNavigationSelectItem item = null;
            SelectExpandClause currentSelectExpandClause = null;
            SelectExpandClause selectExpandClause = null;
            bool levelsEncounteredInInnerExpand = false;
            bool isMaxLevelInInnerExpand = false;
            var entityType = expandItem.NavigationSource.EntityType();
            IEdmNavigationProperty navigationProperty =
                (expandItem.PathToNavigationProperty.LastSegment as NavigationPropertySegment).NavigationProperty;
            ModelBoundQuerySettings nestQuerySettings = EdmLibHelpers.GetModelBoundQuerySettings(navigationProperty,
                navigationProperty.ToEntityType(),
                Context.Model);

            // Try different expansion depth until expandItem.SelectAndExpand is successfully expanded
            while (selectExpandClause == null && level > 0)
            {
                selectExpandClause = ProcessLevels(
                        expandItem.SelectAndExpand,
                        levelsMaxLiteralExpansionDepth - level,
                        nestQuerySettings,
                        out levelsEncounteredInInnerExpand,
                        out isMaxLevelInInnerExpand);
                level--;
            }

            if (selectExpandClause == null)
            {
                return null;
            }

            // Correct level value
            level++;
            List<SelectItem> originAutoSelectItems;
            List<SelectItem> originAutoExpandItems;
            int maxDepth = GetMaxExpandDepth(querySettings, navigationProperty.Name);
            if (maxDepth == 0 || levelsMaxLiteralExpansionDepth > maxDepth)
            {
                maxDepth = levelsMaxLiteralExpansionDepth;
            }

            GetAutoSelectExpandItems(
                entityType,
                Context.Model,
                expandItem.NavigationSource,
                selectExpandClause.AllSelected,
                nestQuerySettings,
                maxDepth - 1,
                out originAutoSelectItems,
                out originAutoExpandItems);
            if (expandItem.SelectAndExpand.SelectedItems.Any(it => it is PathSelectItem))
            {
                originAutoSelectItems.Clear();
            }

            if (level > 1)
            {
                RemoveSameExpandItem(navigationProperty, originAutoExpandItems);
            }

            List<SelectItem> autoExpandItems = new List<SelectItem>(originAutoExpandItems);
            bool hasAutoSelectExpandInExpand = (originAutoSelectItems.Count() + originAutoExpandItems.Count() != 0);
            bool allSelected = originAutoSelectItems.Count == 0 && selectExpandClause.AllSelected;

            while (level > 0)
            {
                autoExpandItems = RemoveExpandItemExceedMaxDepth(maxDepth - level, originAutoExpandItems);
                if (item == null)
                {
                    if (hasAutoSelectExpandInExpand)
                    {
                        currentSelectExpandClause = new SelectExpandClause(
                            new SelectItem[] { }.Concat(selectExpandClause.SelectedItems)
                                .Concat(originAutoSelectItems).Concat(autoExpandItems),
                            allSelected);
                    }
                    else
                    {
                        currentSelectExpandClause = selectExpandClause;
                    }
                }
                else if (selectExpandClause.AllSelected)
                {
                    // Concat the processed items
                    currentSelectExpandClause = new SelectExpandClause(
                        new SelectItem[] { item }.Concat(selectExpandClause.SelectedItems)
                            .Concat(originAutoSelectItems).Concat(autoExpandItems),
                        allSelected);
                }
                else
                {
                    // PathSelectItem is needed for the expanded item if AllSelected is false.
                    PathSelectItem pathSelectItem = new PathSelectItem(
                        new ODataSelectPath(expandItem.PathToNavigationProperty));

                    // Keep default SelectItems before expanded item to keep consistent with normal SelectExpandClause
                    SelectItem[] items = new SelectItem[] { item, pathSelectItem };
                    currentSelectExpandClause = new SelectExpandClause(
                        new SelectItem[] { }.Concat(selectExpandClause.SelectedItems)
                            .Concat(items)
                            .Concat(originAutoSelectItems).Concat(autoExpandItems),
                        allSelected);
                }

                // Construct a new ExpandedNavigationSelectItem with current SelectExpandClause.
                item = new ExpandedNavigationSelectItem(
                    expandItem.PathToNavigationProperty,
                    expandItem.NavigationSource,
                    currentSelectExpandClause);

                level--;

                // Need expand and construct selectExpandClause every time if it is max level in inner expand
                if (isMaxLevelInInnerExpand)
                {
                    selectExpandClause = ProcessLevels(
                        expandItem.SelectAndExpand,
                        levelsMaxLiteralExpansionDepth - level,
                        nestQuerySettings,
                        out levelsEncounteredInInnerExpand,
                        out isMaxLevelInInnerExpand);
                }
            }

            levelsEncounteredInExpand = levelsEncounteredInExpand || levelsEncounteredInInnerExpand || hasAutoSelectExpandInExpand;
            isMaxLevelInExpand = isMaxLevelInExpand || isMaxLevelInInnerExpand;

            return item;
        }

        private static List<SelectItem> RemoveExpandItemExceedMaxDepth(int depth, IEnumerable<SelectItem> autoExpandItems)
        {
            List<SelectItem> selectItems = new List<SelectItem>();
            if (depth <= 0)
            {
                foreach (SelectItem autoSelectItem in autoExpandItems)
                {
                    if (!(autoSelectItem is ExpandedNavigationSelectItem))
                    {
                        selectItems.Add(autoSelectItem);
                    }
                }
            }
            else
            {
                foreach (var autoExpandItem in autoExpandItems)
                {
                    ExpandedNavigationSelectItem expandItem = autoExpandItem as ExpandedNavigationSelectItem;
                    if (expandItem != null)
                    {
                        SelectExpandClause selectExpandClause =
                            new SelectExpandClause(
                                RemoveExpandItemExceedMaxDepth(depth - 1, expandItem.SelectAndExpand.SelectedItems),
                                expandItem.SelectAndExpand.AllSelected);
                        expandItem = new ExpandedNavigationSelectItem(expandItem.PathToNavigationProperty,
                            expandItem.NavigationSource, selectExpandClause);
                        selectItems.Add(expandItem);
                    }
                    else
                    {
                        selectItems.Add(autoExpandItem);
                    }
                }
            }

            return selectItems;
        }

        private static void RemoveSameExpandItem(IEdmNavigationProperty navigationProperty, List<SelectItem> autoExpandItems)
        {
            for (int i = 0; i < autoExpandItems.Count; i++)
            {
                ExpandedNavigationSelectItem expandItem = autoExpandItems[i] as ExpandedNavigationSelectItem;
                IEdmNavigationProperty autoExpandNavigationProperty =
                    (expandItem.PathToNavigationProperty.LastSegment as NavigationPropertySegment).NavigationProperty;
                if (navigationProperty.Name.Equals(autoExpandNavigationProperty.Name))
                {
                    autoExpandItems.RemoveAt(i);
                    return;
                }
            }
        }

        private static int GetMaxExpandDepth(ModelBoundQuerySettings querySettings, string propertyName)
        {
            int result = 0;
            if (querySettings != null)
            {
                ExpandConfiguration expandConfiguration;
                if (querySettings.ExpandConfigurations.TryGetValue(propertyName, out expandConfiguration))
                {
                    result = expandConfiguration.MaxDepth;
                }
                else
                {
                    if (querySettings.DefaultExpandType.HasValue &&
                        querySettings.DefaultExpandType != SelectExpandType.Disabled)
                    {
                        result = querySettings.DefaultMaxDepth;
                    }
                }
            }

            return result;
        }
    }
}
