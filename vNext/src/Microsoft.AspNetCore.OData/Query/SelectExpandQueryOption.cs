// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validators;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents the OData $select and $expand query options.
    /// </summary>
    public class SelectExpandQueryOption
    {
        private readonly IAssemblyProvider _assemblyProvider;
        //private static readonly IAssembliesResolver _defaultAssembliesResolver = new DefaultAssembliesResolver();
        private SelectExpandClause _selectExpandClause;
        private ODataQueryOptionParser _queryOptionParser;
        private int _levelsMaxLiteralExpansionDepth = ODataValidationSettings.DefaultMaxExpansionDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandQueryOption"/> class.
        /// </summary>
        /// <param name="select">The $select query parameter value.</param>
        /// <param name="expand">The $expand query parameter value.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public SelectExpandQueryOption(string select, string expand, ODataQueryContext context,
            ODataQueryOptionParser queryOptionParser, HttpRequest request)
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
            Validator = new SelectExpandQueryValidator();
            _queryOptionParser = queryOptionParser;

            _assemblyProvider = request.AssemblyProvider();
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
            Validator = new SelectExpandQueryValidator();
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
            return ApplyTo(queryable, settings, _assemblyProvider);
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given <see cref="IQueryable"/> using the given
        /// <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="queryable">The original <see cref="IQueryable"/>.</param>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <param name="assembliesProvider">The <see cref="IAssemblyProvider"/> to use.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "stopgap. will be used later.")]
        public IQueryable ApplyTo(IQueryable queryable, ODataQuerySettings settings, IAssemblyProvider assembliesProvider)
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

            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = settings;
            if (settings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation = HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(queryable);
            }

            return SelectExpandBinder.Bind(queryable, updatedSettings, assembliesProvider, this);
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given entity using the given <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public object ApplyTo(object entity, ODataQuerySettings settings)
        {
            return ApplyTo(entity, settings, _assemblyProvider);
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given entity using the given <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <param name="assembliesProvider">The <see cref="IAssembliesResolver"/> to use.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public object ApplyTo(object entity, ODataQuerySettings settings, IAssemblyProvider assembliesProvider)
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

            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = settings;
            if (settings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation = HandleNullPropagationOption.True;
            }

            return SelectExpandBinder.Bind(entity, updatedSettings, assembliesProvider, this);
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
            return ProcessLevels(SelectExpandClause, LevelsMaxLiteralExpansionDepth, out levelsEncountered, out isMaxLevel);
        }

        // Process $levels in SelectExpandClause.
        private static SelectExpandClause ProcessLevels(
            SelectExpandClause selectExpandClause,
            int levelsMaxLiteralExpansionDepth,
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
        private static IEnumerable<SelectItem> ProcessLevels(
            IEnumerable<SelectItem> selectItems,
            int levelsMaxLiteralExpansionDepth,
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

        // Process $levels in ExpandedNavigationSelectItem.
        private static ExpandedNavigationSelectItem ProcessLevels(
            ExpandedNavigationSelectItem expandItem,
            int levelsMaxLiteralExpansionDepth,
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

            // Try diffent expansion depth until expandItem.SelectAndExpand is successfully expanded
            while (selectExpandClause == null && level > 0)
            {
                selectExpandClause = ProcessLevels(
                        expandItem.SelectAndExpand,
                        levelsMaxLiteralExpansionDepth - level,
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

            while (level > 0)
            {
                if (item == null)
                {
                    currentSelectExpandClause = selectExpandClause;
                }
                else if (selectExpandClause.AllSelected)
                {
                    // Concat the processed items
                    currentSelectExpandClause = new SelectExpandClause(
                        new SelectItem[] { item }.Concat(selectExpandClause.SelectedItems),
                        selectExpandClause.AllSelected);
                }
                else
                {
                    // PathSelectItem is needed for the expanded item if AllSelected is false. 
                    PathSelectItem pathSelectItem = new PathSelectItem(
                        new ODataSelectPath(expandItem.PathToNavigationProperty));

                    // Keep default SelectItems before expanded item to keep consistent with normal SelectExpandClause 
                    SelectItem[] items = new SelectItem[] { item, pathSelectItem };
                    currentSelectExpandClause = new SelectExpandClause(
                        new SelectItem[] { }.Concat(selectExpandClause.SelectedItems).Concat(items),
                        selectExpandClause.AllSelected);
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
                        out levelsEncounteredInInnerExpand,
                        out isMaxLevelInInnerExpand);
                }
            }

            levelsEncounteredInExpand = levelsEncounteredInExpand || levelsEncounteredInInnerExpand;
            isMaxLevelInExpand = isMaxLevelInExpand || isMaxLevelInInnerExpand;

            return item;
        }
    }
}
