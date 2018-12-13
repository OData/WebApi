using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Default implementation of SkipTokenHandler for the service. 
    /// </summary>
    public class DefaultSkipTokenHandler : SkipTokenHandler
    {
        private IDictionary<string, object> _propertyValuePairs;
        private const char CommaDelimiter = ',';

        /// <summary>
        /// Constructor for DefaultSkipTokenHandler - Sets the Property Delimiter
        /// </summary>
        public DefaultSkipTokenHandler()
        {
            PropertyDelimiter = ':';
        }

        /// <summary>
        /// Process SkipToken Value to create string key - object value collection 
        /// </summary>
        /// <param name="rawValue"></param>
        public override IDictionary<string, object> ProcessSkipTokenValue(string rawValue)
        {
            _propertyValuePairs = new Dictionary<string, object>();
            string[] keyValues = rawValue.Split(CommaDelimiter);
            foreach (string keyAndValue in keyValues)
            {
                string[] pieces = keyAndValue.Split(new char[] { PropertyDelimiter }, 2);
                if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[1]))
                {
                    object value = null;
                    if (pieces[1].StartsWith("'enumType'"))
                    {
                        string enumValue = pieces[1].Remove(0, 10);
                        IEdmTypeReference type = EdmLibHelpers.GetTypeReferenceOfProperty(Context.Model, Context.ElementClrType, pieces[0]);
                        value = ODataUriUtils.ConvertFromUriLiteral(enumValue, ODataVersion.V401, Context.Model, type);
                    }
                    else
                    {
                        value = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401);
                    }
                    if (!String.IsNullOrWhiteSpace(pieces[0]))
                    {
                        _propertyValuePairs.Add(pieces[0], value);
                    }
                }
            }
            return _propertyValuePairs;
        }

        /// <summary>
        /// Returns the URI for NextPageLink
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="context">Serializer context</param>
        /// <returns></returns>
        public override Uri GenerateNextPageLink(Object lastMember, ODataSerializerContext context)
        {
            if (context == null || lastMember == null)
            {
                return null;
            }
            IEdmModel model = context.Model;
            IList<OrderByNode> orderByNodes = null;
            if (context.QueryOptions.OrderBy != null)
            {
                orderByNodes = context.QueryOptions.OrderBy.OrderByNodes;
            }

            Func<object, string> skipTokenGenerator = (obj) =>
            {
                return GenerateSkipTokenValue(obj, model, orderByNodes);
            };
            return context.InternalRequest.GetNextPageLink(context.InternalRequest.Context.PageSize, lastMember, skipTokenGenerator);
        }

        /// <summary>
        /// Returns a function that converts an object to a skiptoken value string
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">QueryOption </param>
        /// <returns></returns>
        public override string GenerateSkipTokenValue(Object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
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
                skipTokenvalue += property.Name + PropertyDelimiter + uriLiteral + (islast ? String.Empty : CommaDelimiter.ToString());
                count++;
            }
            return skipTokenvalue;
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

                IList<IEdmProperty> orderByProps = orderByNodes.OfType<OrderByPropertyNode>().Select(p => p.Property).AsIList();
                foreach (IEdmProperty subKey in key)
                {
                    orderByProps.Add(subKey);
                }

                return orderByProps.AsEnumerable();
            }
            return key.AsEnumerable();
        }

        /// <summary>
        /// Delimiter used to separate property and value, making it a public property for the purpose of testing
        /// </summary>
        public char PropertyDelimiter { get; set; }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes)
        {
            return ApplyToCore(query, querySettings, orderByNodes) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderByNodes">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes)
        {
            return ApplyToCore(query, querySettings, orderByNodes);
        }

        private IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }
            ExpressionBinderBase binder = new FilterBinder(Context.RequestContainer);
            IDictionary<string, OrderByDirection> directionMap = PopulateDirections(orderByNodes);
            bool parameterizeConstant = querySettings.EnableConstantParameterization;
            ParameterExpression param = Expression.Parameter(Context.ElementClrType);
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
            foreach (KeyValuePair<string, object> item in _propertyValuePairs)
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
