// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    }
}
