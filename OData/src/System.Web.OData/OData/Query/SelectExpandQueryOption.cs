// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Query.Expressions;
using System.Web.OData.Query.Validators;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents the OData $select and $expand query options.
    /// </summary>
    public class SelectExpandQueryOption
    {
        private SelectExpandClause _selectExpandClause;
        private ODataQueryOptionParser _queryOptionParser;
        private int _levelsMaxLiteralExpansionDepth = ODataValidationSettings.DefaultMaxExpansionDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandQueryOption"/> class.
        /// </summary>
        /// <param name="select">The $select query parameter value.</param>
        /// <param name="expand">The $select query parameter value.</param>
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
            Validator = new SelectExpandQueryValidator();
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

            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = settings;
            if (settings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation = HandleNullPropagationOption.True;
            }

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
            return ProcessLevels(SelectExpandClause, LevelsMaxLiteralExpansionDepth, out levelsEncountered);
        }

        // Process $levels in SelectExpandClause.
        private static SelectExpandClause ProcessLevels(
            SelectExpandClause selectExpandClause,
            int levelsMaxLiteralExpansionDepth,
            out bool levelsEncountered)
        {
            levelsEncountered = false;

            if (selectExpandClause == null)
            {
                return null;
            }

            // Process $levels in SelectItems of SelectExpandClause.
            IEnumerable<SelectItem> selectItems = ProcessLevels(
                selectExpandClause.SelectedItems,
                levelsMaxLiteralExpansionDepth,
                out levelsEncountered);

            if (levelsEncountered)
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
            out bool levelsEncountered)
        {
            levelsEncountered = false;
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
                    // Process $levels in ExpandedNavigationSelectItem.
                    ExpandedNavigationSelectItem expandItem = ProcessLevels(
                        item,
                        levelsMaxLiteralExpansionDepth,
                        out levelsEncouteredInExpand);
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
            out bool levelsEncounteredInExpand)
        {
            // Call ProcessLevels on SelectExpandClause recursively.
            SelectExpandClause selectExpandClause = ProcessLevels(
                expandItem.SelectAndExpand,
                levelsMaxLiteralExpansionDepth - 1,
                out levelsEncounteredInExpand);

            if (expandItem.LevelsOption == null)
            {
                if (levelsEncounteredInExpand)
                {
                    return new ExpandedNavigationSelectItem(
                        expandItem.PathToNavigationProperty,
                        expandItem.NavigationSource,
                        selectExpandClause);
                }
                else
                {
                    // Return the original ExpandedNavigationSelectItem if no $levels is found.
                    return expandItem;
                }
            }

            // There is $levels in current ExpandedNavigationSelectItem.
            levelsEncounteredInExpand = true;
            int level = expandItem.LevelsOption.IsMaxLevel ?
                levelsMaxLiteralExpansionDepth :
                (int)expandItem.LevelsOption.Level;

            if (level <= 0)
            {
                // Do not expand if $levels is equal to 0.
                return null;
            }

            // Initialize current SelectExpandClause with processed SelectExpandClause.
            SelectExpandClause currentSelectExpandClause = selectExpandClause;
            ExpandedNavigationSelectItem item = null;

            // Construct new ExpandedNavigationSelectItem with recursive expansion.
            while (level > 0)
            {
                // Construct a new ExpandedNavigationSelectItem with current SelectExpandClause.
                item = new ExpandedNavigationSelectItem(
                    expandItem.PathToNavigationProperty,
                    expandItem.NavigationSource,
                    currentSelectExpandClause);

                // Update current SelectExpandClause with the new ExpandedNavigationSelectItem.
                if (selectExpandClause.AllSelected)
                {
                    currentSelectExpandClause = new SelectExpandClause(
                        new[] { item }.Concat(selectExpandClause.SelectedItems),
                        selectExpandClause.AllSelected);
                }
                else
                {
                    // PathSelectItem is needed for the expanded item if AllSelected is false. 
                    PathSelectItem pathSelectItem = new PathSelectItem(
                        new ODataSelectPath(expandItem.PathToNavigationProperty));
                    currentSelectExpandClause = new SelectExpandClause(
                        new SelectItem[] { item, pathSelectItem }.Concat(selectExpandClause.SelectedItems),
                        selectExpandClause.AllSelected);
                }

                level--;
            }

            return item;
        }
    }
}
