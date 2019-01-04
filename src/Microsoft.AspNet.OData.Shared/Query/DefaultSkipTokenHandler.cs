// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Default implementation of SkipTokenHandler for the service. 
    /// </summary>
    public class DefaultSkipTokenHandler : SkipTokenHandler
    {
        private const char CommaDelimiter = ',';
        private static char propertyDelimiter = ':';

        /// <summary>
        /// Constructor for DefaultSkipTokenHandler - Sets the Property Delimiter
        /// </summary>
        public DefaultSkipTokenHandler()
        {
            IsDeltaFeedSupported = false;
        }

        /// <summary>
        /// Constructor for Unit testing purposes - Sets the Property Delimiter
        /// </summary>
        public DefaultSkipTokenHandler(char delimiter)
            : this()
        {
            propertyDelimiter = delimiter;
        }

        /// <summary>
        /// Returns the URI for NextPageLink
        /// </summary>
        /// <param name="baseUri">BaseUri for nextlink. It should be request URI for top level resource and navigationlink for nested resource.</param>
        /// <param name="instance">Instance based on which SkipToken value will be generated.</param>
        /// <param name="pageSize">Maximum number of records in the set of partial results for a resource.</param>
        /// <param name="context">Serializer context</param>
        /// <returns></returns>
        public override Uri GenerateNextPageLink(Uri baseUri, int pageSize, Object instance, ODataSerializerContext context)
        {
            if (context == null || instance == null)
            {
                return null;
            }

            Func<object, string> skipTokenGenerator = null;
            IList<OrderByNode> orderByNodes = null;
            ExpandedReferenceSelectItem expandedItem = context.ExpandedSelectItem;
            IEdmModel model = context.Model;

            DefaultQuerySettings settings = context.QueryOptions.Context.DefaultQuerySettings;
            if (settings.EnableSkipToken)
            {
                if (expandedItem != null)
                {
                    if (expandedItem.OrderByOption != null)
                    {
                        orderByNodes = OrderByNode.CreateCollection(expandedItem.OrderByOption);
                    }

                    skipTokenGenerator = (obj) =>
                    {
                        return GenerateSkipTokenValue(obj, model, orderByNodes);
                    };
                    
                    return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
                }

                if (context.QueryOptions != null && context.QueryOptions.OrderBy != null)
                {
                    orderByNodes = context.QueryOptions.OrderBy.OrderByNodes;
                }

                skipTokenGenerator = (obj) =>
                {
                    return GenerateSkipTokenValue(obj, model, orderByNodes);
                };
            }
            return context.InternalRequest.GetNextPageLink(pageSize, instance, skipTokenGenerator);
        }

        /// <summary>
        /// Returns a function that converts an object to a skiptoken value string
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">QueryOption </param>
        /// <returns></returns>
        protected virtual string GenerateSkipTokenValue(Object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
        {
            object value;
            if (lastMember == null)
            {
                return String.Empty;
            }
            IEnumerable<IEdmProperty> propertiesForSkipToken = GetPropertiesForSkipToken(lastMember, model, orderByNodes);

            String skipTokenvalue = String.Empty;
            if (propertiesForSkipToken == null)
            {
                return skipTokenvalue;
            }

            int count = 0;
            int lastIndex = propertiesForSkipToken.Count() - 1;
            foreach (IEdmProperty property in propertiesForSkipToken)
            {
                bool islast = count == lastIndex;
                IEdmStructuredObject obj = lastMember as IEdmStructuredObject;
                if (obj != null)
                {
                    obj.TryGetPropertyValue(property.Name, out value);
                }
                else
                {
                    value = lastMember.GetType().GetProperty(property.Name).GetValue(lastMember);
                }

                String uriLiteral = String.Empty;
                if (value == null)
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401);
                }
                else if (TypeHelper.IsEnum(value.GetType()))
                {
                    ODataEnumValue enumValue = new ODataEnumValue(value.ToString(), value.GetType().FullName);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V401, model);
                    uriLiteral = "'enumType'" + uriLiteral;
                }
                else
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401, model);
                }
                skipTokenvalue += property.Name + propertyDelimiter + uriLiteral + (islast ? String.Empty : CommaDelimiter.ToString());
                count++;
            }
            return skipTokenvalue;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="skipTokenQueryOption">The skiptoken query option which needs to be applied to this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable<T> ApplyTo<T>(IQueryable<T> query, SkipTokenQueryOption skipTokenQueryOption)
        {
            if (skipTokenQueryOption == null)
            {
                throw Error.ArgumentNullOrEmpty("skipTokenQueryOption");
            }

            ODataQuerySettings querySettings = skipTokenQueryOption.QuerySettings;
            IList<OrderByNode> orderByNodes = skipTokenQueryOption.OrderByNodes;
            return ApplyToCore(query, querySettings, orderByNodes, skipTokenQueryOption.Context, skipTokenQueryOption.RawValue) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="skipTokenQueryOption">The skiptoken query option which needs to be applied to this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, SkipTokenQueryOption skipTokenQueryOption)
        {
            if (skipTokenQueryOption == null)
            {
                throw Error.ArgumentNullOrEmpty("skipTokenQueryOption");
            }

            ODataQuerySettings querySettings = skipTokenQueryOption.QuerySettings;
            IList<OrderByNode> orderByNodes = skipTokenQueryOption.OrderByNodes;
            return ApplyToCore(query, querySettings, orderByNodes, skipTokenQueryOption.Context, skipTokenQueryOption.RawValue);
        }

        private static IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes, ODataQueryContext context, string skipTokenRawValue)
        {
            if (context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            IDictionary<string, OrderByDirection> directionMap = PopulateDirections(orderByNodes);
            IDictionary<string, object> propertyValuePairs = PopulatePropertyValuePairs(skipTokenRawValue, context);
            ExpressionBinderBase binder = new FilterBinder(context.RequestContainer);

            bool parameterizeConstant = querySettings.EnableConstantParameterization;
            ParameterExpression param = Expression.Parameter(context.ElementClrType);
            Expression where = null;
            /* We will create a where lambda of the following form -
             * Where (Prop1>Value1)
             * OR (Prop1=Value1 AND Prop2>Value2)
             * OR (Prop1=Value1 AND Prop2=Value2 AND Prop3>Value3)
             * and so on...
             * Adding the first true to simplify implementation.
             */
            Expression lastEquality = null;
            bool firstProperty = true;

            foreach (KeyValuePair<string, object> item in propertyValuePairs)
            {
                string key = item.Key;
                MemberExpression property = Expression.Property(param, key);
                object value = item.Value;

                Expression compare = null;
                ODataEnumValue enumValue = value as ODataEnumValue;
                if (enumValue != null)
                {
                    value = enumValue.Value;
                }
                Expression constant = parameterizeConstant ? LinqParameterContainer.Parameterize(value.GetType(), value) : Expression.Constant(value);
                if (directionMap.ContainsKey(key))
                {
                    compare = directionMap[key] == OrderByDirection.Descending ? binder.CreateBinaryExpression(BinaryOperatorKind.LessThan, property, constant, true) : binder.CreateBinaryExpression(BinaryOperatorKind.GreaterThan, property, constant, true);
                }
                else
                {
                    compare = binder.CreateBinaryExpression(BinaryOperatorKind.GreaterThan, property, constant, true);
                }

                if (firstProperty)
                {
                    lastEquality = binder.CreateBinaryExpression(BinaryOperatorKind.Equal, property, constant, true);
                    where = compare;
                    firstProperty = false;
                }
                else
                {
                    Expression condition = Expression.AndAlso(lastEquality, compare);
                    where = where == null ? condition : Expression.OrElse(where, condition);
                    lastEquality = Expression.AndAlso(lastEquality, binder.CreateBinaryExpression(BinaryOperatorKind.Equal, property, constant, true));
                }
            }

            Expression whereLambda = Expression.Lambda(where, param);
            return ExpressionHelpers.Where(query, whereLambda, query.ElementType);
        }

        private static IDictionary<string, object> PopulatePropertyValuePairs(string value, ODataQueryContext context)
        {
            Contract.Assert(context != null);

            IDictionary<string, object> propertyValuePairs = new Dictionary<string, object>();
            string[] keyValues = value.Split(CommaDelimiter);
            foreach (string keyAndValue in keyValues)
            {
                string[] pieces = keyAndValue.Split(new char[] { propertyDelimiter }, 2);
                if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[1]))
                {
                    object propValue = null;
                    if (pieces[1].StartsWith("'enumType'"))
                    {
                        string enumValue = pieces[1].Remove(0, 10);
                        IEdmTypeReference type = EdmLibHelpers.GetTypeReferenceOfProperty(context.Model, context.ElementClrType, pieces[0]);
                        propValue = ODataUriUtils.ConvertFromUriLiteral(enumValue, ODataVersion.V401, context.Model, type);
                    }
                    else
                    {
                        propValue = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401);
                    }
                    if (!String.IsNullOrWhiteSpace(pieces[0]))
                    {
                        propertyValuePairs.Add(pieces[0], propValue);
                    }
                }
            }

            return propertyValuePairs;
        }

        private static IEnumerable<IEdmProperty> GetPropertiesForSkipToken(object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
        {
            IEdmType edmType = GetTypeFromObject(lastMember, model);
            IEdmEntityType entity = edmType as IEdmEntityType;
            if (entity == null)
            {
                return null;
            }

            IList<IEdmProperty> key = entity.Key().AsIList<IEdmProperty>();
            if (orderByNodes != null)
            {
                OrderByOpenPropertyNode orderByOpenType = orderByNodes.OfType<OrderByOpenPropertyNode>().LastOrDefault();
                if (orderByOpenType != null)
                {
                    //SkipToken will not support ordering on dynamic properties
                    return null;
                }

                List<IEdmProperty> orderByProps = orderByNodes.OfType<OrderByPropertyNode>().Select(p => p.Property).AsList();
                foreach (IEdmProperty subKey in key)
                {
                    if (!orderByProps.Contains(subKey))
                    {
                        orderByProps.Add(subKey);
                    }
                }

                return orderByProps.AsEnumerable();
            }
            return key.AsEnumerable();
        }

        private static IDictionary<string, OrderByDirection> PopulateDirections(IList<OrderByNode> orderByNodes)
        {
            IDictionary<string, OrderByDirection> directions = new Dictionary<string, OrderByDirection>();
            if (orderByNodes == null)
            {
                return directions;
            }

            foreach (OrderByPropertyNode node in orderByNodes)
            {
                if (node != null)
                {
                    directions[node.Property.Name] = node.Direction;
                }
            }
            return directions;
        }

        private static IEdmType GetTypeFromObject(object obj, IEdmModel model)
        {
            SelectExpandWrapper selectExpand = obj as SelectExpandWrapper;
            if (selectExpand != null)
            {
                IEdmTypeReference typeReference = selectExpand.GetEdmType();
                return typeReference.Definition;
            }

            Type clrType = obj.GetType();
            return EdmLibHelpers.GetEdmType(model, clrType);
        }
    }
}
