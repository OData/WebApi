﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validators;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary> 
    /// This defines a $orderby OData query option for querying. 
    /// </summary> 
    public class OrderByQueryOption
    {
        private OrderByClause _orderByClause;
        private IList<OrderByNode> _orderByNode;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $orderby value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets or sets the OrderBy Query Validator.
        /// </summary>
        public OrderByQueryValidator Validator { get; set; }

        public OrderByClause OrderByClause
        {
            get
            {
                if (_orderByClause == null)
                {
                    _orderByClause = _queryOptionParser.ParseOrderBy();
                    _orderByClause = TranslateParameterAlias(_orderByClause);
                }
                return _orderByClause;
            }
        }

        public IList<OrderByNode> OrderByNodes
        {
            get
            {
                if (_orderByNode == null)
                {
                    _orderByNode = OrderByNode.CreateCollection(OrderByClause);
                }
                return _orderByNode;
            }
        }

        public OrderByQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            Validator = new OrderByQueryValidator();
            _queryOptionParser = queryOptionParser;
        }

        /// <summary>
        /// Apply the $orderby query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the orderby query has been applied to.</returns>
        public IOrderedQueryable<T> ApplyTo<T>(IQueryable<T> query)
        {
            return ApplyToCore(query, new ODataQuerySettings()) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $orderby query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the orderby query has been applied to.</returns>
        public IOrderedQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $orderby query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the orderby query has been applied to.</returns>
        public IOrderedQueryable ApplyTo(IQueryable query)
        {
            return ApplyToCore(query, new ODataQuerySettings());
        }

        /// <summary>
        /// Apply the $orderby query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the orderby query has been applied to.</returns>
        public IOrderedQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            return ApplyToCore(query, querySettings);
        }

        /// <summary>
        /// Validate the orderby query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
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

        private IOrderedQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            ICollection<OrderByNode> nodes = OrderByNodes;

            bool alreadyOrdered = false;
            IQueryable querySoFar = query;

            HashSet<IEdmProperty> propertiesSoFar = new HashSet<IEdmProperty>();
            HashSet<string> openPropertiesSoFar = new HashSet<string>();
            bool orderByItSeen = false;

            foreach (OrderByNode node in nodes)
            {
                OrderByPropertyNode propertyNode = node as OrderByPropertyNode;
                OrderByOpenPropertyNode openPropertyNode = node as OrderByOpenPropertyNode;

                if (propertyNode != null)
                {
                    IEdmProperty property = propertyNode.Property;
                    OrderByDirection direction = propertyNode.Direction;

                    // This check prevents queries with duplicate properties (e.g. $orderby=Id,Id,Id,Id...) from causing stack overflows
                    if (propertiesSoFar.Contains(property))
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateProperty, property.Name));
                    }
                    propertiesSoFar.Add(property);

                    if (propertyNode.OrderByClause != null)
                    {
                        querySoFar = AddOrderByQueryForProperty(query, querySettings, propertyNode.OrderByClause, querySoFar, direction, alreadyOrdered);
                    }
                    else
                    {
                        querySoFar = ExpressionHelpers.OrderByProperty(querySoFar, Context.Model, property, direction, Context.ElementClrType, alreadyOrdered);
                    }
                    alreadyOrdered = true;
                }
                else if (openPropertyNode != null)
                {
                    // This check prevents queries with duplicate properties (e.g. $orderby=Id,Id,Id,Id...) from causing stack overflows
                    if (openPropertiesSoFar.Contains(openPropertyNode.PropertyName))
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateProperty, openPropertyNode.PropertyName));
                    }
                    openPropertiesSoFar.Add(openPropertyNode.PropertyName);
                    Contract.Assert(openPropertyNode.OrderByClause != null);
                    querySoFar = AddOrderByQueryForProperty(query, querySettings, openPropertyNode.OrderByClause, querySoFar, openPropertyNode.Direction, alreadyOrdered);
                    alreadyOrdered = true;
                }
                else
                {
                    // This check prevents queries with duplicate nodes (e.g. $orderby=$it,$it,$it,$it...) from causing stack overflows
                    if (orderByItSeen)
                    {
                        throw new ODataException(Error.Format(SRResources.OrderByDuplicateIt));
                    }

                    querySoFar = ExpressionHelpers.OrderByIt(querySoFar, node.Direction, Context.ElementClrType, alreadyOrdered);
                    alreadyOrdered = true;
                    orderByItSeen = true;
                }
            }

            return querySoFar as IOrderedQueryable;
        }

        private IQueryable AddOrderByQueryForProperty(IQueryable query, ODataQuerySettings querySettings,
            OrderByClause orderbyClause, IQueryable querySoFar, OrderByDirection direction, bool alreadyOrdered)
        {
            // Ensure we have decided how to handle null propagation
            ODataQuerySettings updatedSettings = querySettings;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                updatedSettings = new ODataQuerySettings(updatedSettings);
                updatedSettings.HandleNullPropagation =
                    HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query);
            }

            LambdaExpression orderByExpression =
                FilterBinder.Bind(orderbyClause, Context.ElementClrType, Context.Model, updatedSettings);
            querySoFar = ExpressionHelpers.OrderBy(querySoFar, orderByExpression, direction, Context.ElementClrType,
                alreadyOrdered);
            return querySoFar;
        }

        private OrderByClause TranslateParameterAlias(OrderByClause orderBy)
        {
            if (orderBy == null)
            {
                return null;
            }

            SingleValueNode orderByExpression = orderBy.Expression.Accept(
                new ParameterAliasNodeTranslator(_queryOptionParser.ParameterAliasNodes)) as SingleValueNode;
            orderByExpression = orderByExpression ?? new ConstantNode(null, "null");

            return new OrderByClause(
                TranslateParameterAlias(orderBy.ThenBy),
                orderByExpression,
                orderBy.Direction,
                orderBy.RangeVariable);
        }
    }
}
