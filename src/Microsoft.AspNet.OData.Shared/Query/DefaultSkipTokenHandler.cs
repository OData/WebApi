//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
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
        private static char propertyDelimiter = '-';
        internal static DefaultSkipTokenHandler Instance = new DefaultSkipTokenHandler();

        /// <summary>
        /// Returns the URI for NextPageLink
        /// </summary>
        /// <param name="baseUri">BaseUri for nextlink. It should be request URI for top level resource and navigation link for nested resource.</param>
        /// <param name="pageSize">Maximum number of records in the set of partial results for a resource.</param>
        /// <param name="instance">Instance based on which SkipToken value will be generated.</param>
        /// <param name="context">Serializer context</param>
        /// <returns>Returns the URI for NextPageLink. If a null object is passed for the instance, resorts to the default paging mechanism of using $skip and $top.</returns>
        public override Uri GenerateNextPageLink(Uri baseUri, int pageSize, Object instance, ODataSerializerContext context)
        {
            if (context == null)
            {
                return null;
            }

            if (pageSize <= 0)
            {
                return null;
            }

            Func<object, string> skipTokenGenerator = null;
            IList<OrderByNode> orderByNodes = null;
            ExpandedReferenceSelectItem expandedItem = context.CurrentExpandedSelectItem;
            IEdmModel model = context.Model;

            DefaultQuerySettings settings = context.QueryContext.DefaultQuerySettings;
            if (settings.EnableSkipToken)
            {
                if (expandedItem != null)
                {
                    // Handle Delta resource; currently not value based.
                    if (TypedDelta.IsDeltaOfT(context.ExpandedResource.GetType()))
                    {
                        return GetNextPageHelper.GetNextPageLink(baseUri, pageSize);
                    }

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

            return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
        }

        /// <summary>
        /// Generates a string to be used as the skip token value within the next link.
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">List of orderByNodes used to generate the skiptoken value.</param>
        /// <returns>Value for the skiptoken to be used in the next link.</returns>
        private static string GenerateSkipTokenValue(Object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
        {
            if (lastMember == null)
            {
                return String.Empty;
            }

            IEnumerable<IEdmProperty> propertiesForSkipToken = GetPropertiesForSkipToken(lastMember, model, orderByNodes);
            StringBuilder skipTokenBuilder = new StringBuilder(String.Empty);
            if (propertiesForSkipToken == null)
            {
                return skipTokenBuilder.ToString();
            }

            int count = 0;
            string uriLiteral;
            object value;
            int lastIndex = propertiesForSkipToken.Count() - 1;
            IEdmStructuredObject obj = lastMember as IEdmStructuredObject;

            foreach (IEdmProperty edmProperty in propertiesForSkipToken)
            {
                bool islast = count == lastIndex;
                string clrPropertyName = EdmLibHelpers.GetClrPropertyName(edmProperty, model);
                if (obj != null)
                {
                    obj.TryGetPropertyValue(clrPropertyName, out value);
                }
                else
                {
                    value = lastMember.GetType().GetProperty(clrPropertyName).GetValue(lastMember);
                }

                if (value == null)
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401);
                }
                else if (edmProperty.Type.IsEnum())
                {
                    ODataEnumValue enumValue = new ODataEnumValue(value.ToString(), value.GetType().FullName);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V401, model);
                }
                else if(edmProperty.Type.IsDateTimeOffset() && value is DateTime)
                {
                    var dateTime = (DateTime)value;
                    var dateTimeOffsetValue = TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(dateTimeOffsetValue, ODataVersion.V401, model);
                }
                else
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401, model);
                }

                var encodedUriLiteral = WebUtility.UrlEncode(uriLiteral);

                skipTokenBuilder.Append(edmProperty.Name).Append(propertyDelimiter).Append(encodedUriLiteral).Append(islast ? String.Empty : CommaDelimiter.ToString());
                count++;
            }

            return skipTokenBuilder.ToString();
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="skipTokenQueryOption">The skiptoken query option which needs to be applied to this query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable<T> ApplyTo<T>(IQueryable<T> query, SkipTokenQueryOption skipTokenQueryOption)
        {
            return ApplyTo(query, skipTokenQueryOption);
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
            ODataQueryOptions queryOptions = skipTokenQueryOption.QueryOptions;
            IList<OrderByNode> orderByNodes = null;

            if (queryOptions != null)
            {
                OrderByQueryOption orderBy = queryOptions.GenerateStableOrder();
                if (orderBy != null)
                {
                    orderByNodes = orderBy.OrderByNodes;
                }
            }

            return ApplyToCore(query, querySettings, orderByNodes, skipTokenQueryOption.Context, skipTokenQueryOption.RawValue);
        }

        /// <summary>
        /// Core logic for applying the query option to the IQueryable. 
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">Query setting used for validating the query option.</param>
        /// <param name="orderByNodes">OrderBy information required to correctly apply the query option for default implementation.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="skipTokenRawValue">The raw string value of the skiptoken query parameter.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable.")]
        private static IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes, ODataQueryContext context, string skipTokenRawValue)
        {
            if (context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            IDictionary<string, OrderByDirection> directionMap;
            if (orderByNodes != null)
            {
                directionMap =
                    orderByNodes.OfType<OrderByPropertyNode>().ToDictionary(node => node.Property.Name, node => node.Direction);
            }
            else
            {
                directionMap = new Dictionary<string, OrderByDirection>();
            }

            IDictionary<string, Tuple<object, Type>> propertyValuePairs = PopulatePropertyValuePairs(skipTokenRawValue, context);

            if (propertyValuePairs.Count == 0)
            {
                throw Error.InvalidOperation(SRResources.SkipTokenProcessingError);
            }

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

            foreach (KeyValuePair<string, Tuple<object, Type>> item in propertyValuePairs)
            {
                string key = item.Key;
                MemberExpression property = Expression.Property(param, key);

                object value = item.Value.Item1;

                Type propertyType = item.Value.Item2 ?? value.GetType();
                bool propertyIsNullable = TypeHelper.IsNullable(property.Type);

                Expression compare = null;
                if (value is ODataEnumValue enumValue)
                {
                    value = enumValue.Value;
                    propertyType = value.GetType();
                }
                else if (value is ODataNullValue)
                {
                    value = null;
                    propertyType = property.Type;
                }

                Expression constant = parameterizeConstant ? LinqParameterContainer.Parameterize(propertyType, value) : Expression.Constant(value);

                if (directionMap.ContainsKey(key) && directionMap[key] == OrderByDirection.Descending)
                {
                    // Prop < Value
                    compare = binder.CreateBinaryExpression(
                        binaryOperator: BinaryOperatorKind.LessThan,
                        left: property,
                        right: constant,
                        liftToNull: !propertyIsNullable);

                    if (propertyIsNullable && value != null)
                    {
                        // Prop == null

                        // We only do this when value is NOT null since
                        // ((Prop1 < null) OR (Prop1 == null)) OR ((Prop1 == null) AND (Prop2 > Value2))
                        // doesn't make logical sense
                        Expression condition = binder.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.Equal,
                            left: property,
                            right: parameterizeConstant ? LinqParameterContainer.Parameterize(property.Type, null) : Expression.Constant(null),
                            liftToNull: false);

                        // (Prop < Value) OR (Prop == null)
                        compare = Expression.OrElse(compare, condition);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        // Prop != null

                        // When value is null in the ascending order scenario,
                        // we are aiming for the following expression:
                        // (Prop1 != null) OR ((Prop1 == null) AND (Prop2 > Value2)) ...
                        compare = binder.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.NotEqual,
                            left: property,
                            right: constant,
                            liftToNull: false);
                    }
                    else
                    {
                        // Prop > Value

                        // When value is NOT null in the ascending order scenario,
                        // we are aiming for the following expression:
                        // (Prop1 > Value1) OR ((Prop1 == Value1) AND (Prop2 > Value2)) ...

                        compare = binder.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.GreaterThan,
                            left: property,
                            right: constant,
                            liftToNull: !propertyIsNullable);
                    }
                }

                if (firstProperty)
                {
                    lastEquality = binder.CreateBinaryExpression(
                        binaryOperator: BinaryOperatorKind.Equal,
                        left: property,
                        right: constant,
                        liftToNull: !propertyIsNullable);
                    where = compare;
                    firstProperty = false;
                }
                else
                {
                    Expression condition = Expression.AndAlso(lastEquality, compare);
                    where = Expression.OrElse(where, condition);
                    lastEquality = Expression.AndAlso(lastEquality,
                        binder.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.Equal,
                            left: property,
                            right: constant,
                            liftToNull: !propertyIsNullable));
                }
            }

            Expression whereLambda = Expression.Lambda(where, param);
            return ExpressionHelpers.Where(query, whereLambda, query.ElementType);
        }

        /// <summary>
        /// Generates a dictionary with property name and property values specified in the skiptoken value.
        /// </summary>
        /// <param name="value">The skiptoken string value.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <returns>Dictionary with property name and property value in the skiptoken value.</returns>
        private static IDictionary<string, Tuple<object, Type>> PopulatePropertyValuePairs(string value, ODataQueryContext context)
        {
            Contract.Assert(context != null);

            IDictionary<string, Tuple<object, Type>> propertyValuePairs = new Dictionary<string, Tuple<object, Type>>();
            IList<string> keyValuesPairs = ParseValue(value, CommaDelimiter);

            IEdmStructuredType type = context.ElementType as IEdmStructuredType;
            Debug.Assert(type != null);

            foreach (string pair in keyValuesPairs)
            {
                string[] pieces = pair.Split(new char[] { propertyDelimiter }, 2);
                if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[0]))
                {
                    object propValue = null;

                    IEdmTypeReference propertyType = null;
                    IEdmProperty property = type.FindProperty(pieces[0]);
                    Type propertyClrType = null;
                    if (property != null)
                    {
                        propertyType = property.Type;
                        propertyClrType = EdmLibHelpers.GetClrType(propertyType, context.Model);
                    }

                    propValue = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401, context.Model, propertyType);
                    propertyValuePairs.Add(pieces[0], Tuple.Create(propValue, propertyClrType));
                }
                else
                {
                    throw Error.InvalidOperation(SRResources.SkipTokenParseError);
                }
            }

            return propertyValuePairs;
        }

        private static IList<string> ParseValue(string value, char delim)
        {
            IList<string> results = new List<string>();
            StringBuilder escapedStringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\'' || value[i] == '"')
                {
                    escapedStringBuilder.Append(value[i]);
                    char openingQuoteChar = value[i];
                    i++;
                    while (i < value.Length && value[i] != openingQuoteChar)
                    {
                        escapedStringBuilder.Append(value[i++]);
                    }

                    if (i != value.Length)
                    {
                        escapedStringBuilder.Append(value[i]);
                    }
                }
                else if (value[i] == delim)
                {
                    results.Add(escapedStringBuilder.ToString());
                    escapedStringBuilder.Clear();
                }
                else
                {
                    escapedStringBuilder.Append(value[i]);
                }
            }

            string lastPair = escapedStringBuilder.ToString();
            if (!String.IsNullOrWhiteSpace(lastPair))
            {
                results.Add(lastPair);
            }

            return results;
        }

        /// <summary>
        /// Returns the list of properties that should be used for generating the skiptoken value. 
        /// </summary>
        /// <param name="lastMember">The last record that will be returned in the response.</param>
        /// <param name="model">IEdmModel</param>
        /// <param name="orderByNodes">OrderBy nodes in the original request.</param>
        /// <returns>List of properties that should be used for generating the skiptoken value.</returns>
        private static IEnumerable<IEdmProperty> GetPropertiesForSkipToken(object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
        {
            IEdmType edmType = GetTypeFromObject(lastMember, model);
            IEdmEntityType entity = edmType as IEdmEntityType;
            if (entity == null)
            {
                return null;
            }

            IEnumerable<IEdmProperty> key = entity.Key();
            if (orderByNodes != null)
            {
                if (orderByNodes.OfType<OrderByOpenPropertyNode>().Any())
                {
                    //SkipToken will not support ordering on dynamic properties
                    return null;
                }

                IList<IEdmProperty> orderByProps = orderByNodes.OfType<OrderByPropertyNode>().Select(p => p.Property).AsList();
                foreach (IEdmProperty subKey in key)
                {
                    if (!orderByProps.Contains(subKey))
                    {
                        orderByProps.Add(subKey);
                    }
                }

                return orderByProps.AsEnumerable();
            }

            return key;
        }

        /// <summary>
        /// Gets the EdmType from the Instance which may be a select expand wrapper.
        /// </summary>
        /// <param name="value">Instance for which the edmType needs to be computed.</param>
        /// <param name="model">IEdmModel</param>
        /// <returns>The EdmType of the underlying instance.</returns>
        private static IEdmType GetTypeFromObject(object value, IEdmModel model)
        {
            SelectExpandWrapper selectExpand = value as SelectExpandWrapper;
            if (selectExpand != null)
            {
                IEdmTypeReference typeReference = selectExpand.GetEdmType();
                return typeReference.Definition;
            }

            Type clrType = value.GetType();
            return model.GetTypeMappingCache().GetEdmType(clrType, model)?.Definition;
        }
    }
}
