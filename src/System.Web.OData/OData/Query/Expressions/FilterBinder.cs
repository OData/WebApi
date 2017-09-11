﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $filter parse tree represented by <see cref="FilterClause"/> to
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    public class FilterBinder : ExpressionBinderBase
    {
        private const string ODataItParameterName = "$it";

        private Stack<Dictionary<string, ParameterExpression>> _parametersStack = new Stack<Dictionary<string, ParameterExpression>>();
        private Dictionary<string, ParameterExpression> _lambdaParameters;
        private Type _filterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBinder"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        public FilterBinder(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }

        internal static Expression Bind(IQueryable baseQuery, FilterClause filterClause, Type filterType, IServiceProvider requestContainer)
        {
            if (filterClause == null)
            {
                throw Error.ArgumentNull("filterClause");
            }
            if (filterType == null)
            {
                throw Error.ArgumentNull("filterType");
            }
            if (requestContainer == null)
            {
                throw Error.ArgumentNull("requestContainer");
            }

            FilterBinder binder = requestContainer.GetRequiredService<FilterBinder>();
            binder._filterType = filterType;
            binder.BaseQuery = baseQuery;

            return BindFilterClause(binder, filterClause, filterType);
        }

        internal static LambdaExpression Bind(IQueryable baseQuery, OrderByClause orderBy, Type elementType, IServiceProvider requestContainer)
        {
            Contract.Assert(orderBy != null);
            Contract.Assert(elementType != null);
            Contract.Assert(requestContainer != null);

            FilterBinder binder = requestContainer.GetRequiredService<FilterBinder>();
            binder._filterType = elementType;
            binder.BaseQuery = baseQuery;

            return BindOrderByClause(binder, orderBy, elementType);
        }

        #region For testing purposes only.

        private FilterBinder(
            IEdmModel model,
            IAssembliesResolver assembliesResolver,
            ODataQuerySettings querySettings,
            Type filterType)
            : base(model, assembliesResolver, querySettings)
        {
            _filterType = filterType;
        }

        internal static Expression<Func<TEntityType, bool>> Bind<TEntityType>(FilterClause filterClause, IEdmModel model,
            IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            return Bind(filterClause, typeof(TEntityType), model, assembliesResolver, querySettings) as Expression<Func<TEntityType, bool>>;
        }

        internal static Expression Bind(FilterClause filterClause, Type filterType, IEdmModel model,
            IAssembliesResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            if (filterClause == null)
            {
                throw Error.ArgumentNull("filterClause");
            }
            if (filterType == null)
            {
                throw Error.ArgumentNull("filterType");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            FilterBinder binder = new FilterBinder(model, assembliesResolver, querySettings, filterType);

            return BindFilterClause(binder, filterClause, filterType);
        }

        #endregion

        private static LambdaExpression BindFilterClause(FilterBinder binder, FilterClause filterClause, Type filterType)
        {
            LambdaExpression filter = binder.BindExpression(filterClause.Expression, filterClause.RangeVariable, filterType);
            filter = Expression.Lambda(binder.ApplyNullPropagationForFilterBody(filter.Body), filter.Parameters);

            Type expectedFilterType = typeof(Func<,>).MakeGenericType(filterType, typeof(bool));
            if (filter.Type != expectedFilterType)
            {
                throw Error.Argument("filterType", SRResources.CannotCastFilter, filter.Type.FullName, expectedFilterType.FullName);
            }

            return filter;
        }

        private static LambdaExpression BindOrderByClause(FilterBinder binder, OrderByClause orderBy, Type elementType)
        {
            LambdaExpression orderByLambda = binder.BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType);
            return orderByLambda;
        }

        /// <summary>
        /// Binds a <see cref="QueryNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        public virtual Expression Bind(QueryNode node)
        {
            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            CollectionNode collectionNode = node as CollectionNode;
            SingleValueNode singleValueNode = node as SingleValueNode;

            if (collectionNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.CollectionNavigationNode:
                        CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                    case QueryNodeKind.CollectionPropertyAccess:
                        return BindCollectionPropertyAccessNode(node as CollectionPropertyAccessNode);

                    case QueryNodeKind.CollectionComplexNode:
                        return BindCollectionComplexNode(node as CollectionComplexNode);

                    case QueryNodeKind.CollectionResourceCast:
                        return BindCollectionResourceCastNode(node as CollectionResourceCastNode);

                    case QueryNodeKind.CollectionFunctionCall:
                    case QueryNodeKind.CollectionResourceFunctionCall:
                    case QueryNodeKind.CollectionOpenPropertyAccess:
                    // Unused or have unknown uses.
                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else if (singleValueNode != null)
            {
                switch (node.Kind)
                {
                    case QueryNodeKind.BinaryOperator:
                        return BindBinaryOperatorNode(node as BinaryOperatorNode);

                    case QueryNodeKind.Constant:
                        return BindConstantNode(node as ConstantNode);

                    case QueryNodeKind.Convert:
                        return BindConvertNode(node as ConvertNode);

                    case QueryNodeKind.ResourceRangeVariableReference:
                        return BindRangeVariable((node as ResourceRangeVariableReferenceNode).RangeVariable);

                    case QueryNodeKind.NonResourceRangeVariableReference:
                        return BindRangeVariable((node as NonResourceRangeVariableReferenceNode).RangeVariable);

                    case QueryNodeKind.SingleValuePropertyAccess:
                        return BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode);

                    case QueryNodeKind.SingleComplexNode:
                        return BindSingleComplexNode(node as SingleComplexNode);

                    case QueryNodeKind.SingleValueOpenPropertyAccess:
                        return BindDynamicPropertyAccessQueryNode(node as SingleValueOpenPropertyAccessNode);

                    case QueryNodeKind.UnaryOperator:
                        return BindUnaryOperatorNode(node as UnaryOperatorNode);

                    case QueryNodeKind.SingleValueFunctionCall:
                        return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode);

                    case QueryNodeKind.SingleNavigationNode:
                        SingleNavigationNode navigationNode = node as SingleNavigationNode;
                        return BindNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty);

                    case QueryNodeKind.Any:
                        return BindAnyNode(node as AnyNode);

                    case QueryNodeKind.All:
                        return BindAllNode(node as AllNode);

                    case QueryNodeKind.SingleResourceCast:
                        return BindSingleResourceCastNode(node as SingleResourceCastNode);

                    case QueryNodeKind.SingleResourceFunctionCall:
                        return BindSingleResourceFunctionCallNode(node as SingleResourceFunctionCallNode);

                    case QueryNodeKind.NamedFunctionParameter:
                    case QueryNodeKind.ParameterAlias:
                    case QueryNodeKind.EntitySet:
                    case QueryNodeKind.KeyLookup:
                    case QueryNodeKind.SearchTerm:
                    // Unused or have unknown uses.
                    default:
                        throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
                }
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(FilterBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueOpenPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueOpenPropertyAccessNode"/>.
        /// </summary>
        /// <param name="openNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindDynamicPropertyAccessQueryNode(SingleValueOpenPropertyAccessNode openNode)
        {
            if (EdmLibHelpers.IsDynamicTypeWrapper(_filterType))
            {
                return GetFlattenedPropertyExpression(openNode.Name) ?? Expression.Property(Bind(openNode.Source), openNode.Name);
            }
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);

            var propertyAccessExpression = BindPropertyAccessExpression(openNode, prop);
            var readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            var containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            var nullExpression = Expression.Constant(null);

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                var dynamicDictIsNotNull = Expression.NotEqual(propertyAccessExpression, Expression.Constant(null));
                var dynamicDictIsNotNullAndContainsKey = Expression.AndAlso(dynamicDictIsNotNull, containsKeyExpression);
                return Expression.Condition(
                    dynamicDictIsNotNullAndContainsKey,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
            else
            {
                return Expression.Condition(
                    containsKeyExpression,
                    readDictionaryIndexerExpression,
                    nullExpression);
            }
        }

        private Expression BindPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode, PropertyInfo prop)
        {
            var source = Bind(openNode.Source);
            Expression propertyAccessExpression;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True &&
                IsNullable(source.Type) && source != _lambdaParameters[ODataItParameterName])
            {
                propertyAccessExpression = Expression.Property(RemoveInnerNullPropagation(source), prop.Name);
            }
            else
            {
                propertyAccessExpression = Expression.Property(source, prop.Name);
            }
            return propertyAccessExpression;
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceFunctionCallNode(SingleResourceFunctionCallNode node)
        {
            switch (node.Name)
            {
                case ClrCanonicalFunctions.CastFunctionName:
                    return BindSingleResourceCastFunctionCall(node);
                default:
                    throw Error.NotSupported(SRResources.ODataFunctionNotSupported, node.Name);
            }
        }

        private Expression BindSingleResourceCastFunctionCall(SingleResourceFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 2);

            string targetEdmTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = Model.FindType(targetEdmTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                targetClrType = EdmLibHelpers.GetClrType(targetEdmType.ToEdmTypeReference(false), Model);
            }

            if (arguments[0].Type == targetClrType)
            {
                // We only support to cast Entity type to the same type now.
                return arguments[0];
            }
            else
            {
                // Cast fails and return null.
                return NullConstant;
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleResourceCastNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleResourceCastNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleResourceCastNode(SingleResourceCastNode node)
        {
            IEdmStructuredTypeReference structured = node.StructuredTypeReference;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = EdmLibHelpers.GetClrType(structured, Model);

            Expression source = BindCastSourceNode(node.Source);
            return Expression.TypeAs(source, clrType);
        }

        /// <summary>
        /// Binds a <see cref="CollectionResourceCastNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionResourceCastNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionResourceCastNode(CollectionResourceCastNode node)
        {
            IEdmStructuredTypeReference structured = node.ItemStructuredType;
            Contract.Assert(structured != null, "NS casts can contain only structured types");

            Type clrType = EdmLibHelpers.GetClrType(structured, Model);

            Expression source = BindCastSourceNode(node.Source);
            return OfType(source, clrType);
        }

        private Expression BindCastSourceNode(QueryNode sourceNode)
        {
            Expression source;
            if (sourceNode == null)
            {
                // if the cast is on the root i.e $it (~/Products?$filter=NS.PopularProducts/.....),
                // source would be null. So bind null to '$it'.
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return source;
        }

        private static Expression OfType(Expression source, Type elementType)
        {
            Contract.Assert(source != null);
            Contract.Assert(elementType != null);

            if (IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableOfType.MakeGenericMethod(elementType), source);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableOfType.MakeGenericMethod(elementType), source);
            }
        }

        /// <summary>
        /// Binds a <see cref="IEdmNavigationProperty"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="sourceNode">The node that represents the navigation source.</param>
        /// <param name="navigationProperty">The navigation property to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty)
        {
            Expression source;

            // TODO: bug in uri parser is causing this property to be null for the root property.
            if (sourceNode == null)
            {
                source = _lambdaParameters[ODataItParameterName];
            }
            else
            {
                source = Bind(sourceNode);
            }

            return CreatePropertyAccessExpression(source, navigationProperty);
        }

        /// <summary>
        /// Binds a <see cref="BinaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="BinaryOperatorNode"/>.
        /// </summary>
        /// <param name="binaryOperatorNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode)
        {
            Expression left = Bind(binaryOperatorNode.Left);
            Expression right = Bind(binaryOperatorNode.Right);

            // handle null propagation only if either of the operands can be null
            bool isNullPropagationRequired = QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && (IsNullable(left.Type) || IsNullable(right.Type));
            if (isNullPropagationRequired)
            {
                // |----------------------------------------------------------------|
                // |SQL 3VL truth table.                                            |
                // |----------------------------------------------------------------|
                // |p       |    q      |    p OR q     |    p AND q    |    p = q  |
                // |----------------------------------------------------------------|
                // |True    |   True    |   True        |   True        |   True    |
                // |True    |   False   |   True        |   False       |   False   |
                // |True    |   NULL    |   True        |   NULL        |   NULL    |
                // |False   |   True    |   True        |   False       |   False   |
                // |False   |   False   |   False       |   False       |   True    |
                // |False   |   NULL    |   NULL        |   False       |   NULL    |
                // |NULL    |   True    |   True        |   NULL        |   NULL    |
                // |NULL    |   False   |   NULL        |   False       |   NULL    |
                // |NULL    |   NULL    |   Null        |   NULL        |   NULL    |
                // |--------|-----------|---------------|---------------|-----------|

                // before we start with null propagation, convert the operators to nullable if already not.
                left = ToNullable(left);
                right = ToNullable(right);

                bool liftToNull = true;
                if (left == NullConstant || right == NullConstant)
                {
                    liftToNull = false;
                }

                // Expression trees do a very good job of handling the 3VL truth table if we pass liftToNull true.
                return CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: liftToNull);
            }
            else
            {
                return CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: false);
            }
        }

        /// <summary>
        /// Binds a <see cref="ConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConstantNode"/>.
        /// </summary>
        /// <param name="constantNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConstantNode(ConstantNode constantNode)
        {
            Contract.Assert(constantNode != null);

            // no need to parameterize null's as there cannot be multiple values for null.
            if (constantNode.Value == null)
            {
                return NullConstant;
            }

            Type constantType = EdmLibHelpers.GetClrType(constantNode.TypeReference, Model, AssembliesResolver);
            object value = constantNode.Value;

            if (constantNode.TypeReference != null && constantNode.TypeReference.IsEnum())
            {
                ODataEnumValue odataEnumValue = (ODataEnumValue)value;
                string strValue = odataEnumValue.Value;
                Contract.Assert(strValue != null);

                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
                value = Enum.Parse(constantType, strValue);
            }

            if (constantNode.TypeReference != null &&
                constantNode.TypeReference.IsNullable &&
                (constantNode.TypeReference.IsDate() || constantNode.TypeReference.IsTimeOfDay()))
            {
                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
            }

            if (QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        /// <summary>
        /// Binds a <see cref="ConvertNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConvertNode"/>.
        /// </summary>
        /// <param name="convertNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConvertNode(ConvertNode convertNode)
        {
            Contract.Assert(convertNode != null);
            Contract.Assert(convertNode.TypeReference != null);

            Expression source = Bind(convertNode.Source);

            return CreateConvertExpression(convertNode, source);
        }

        private LambdaExpression BindExpression(SingleValueNode expression, RangeVariable rangeVariable, Type elementType)
        {
            ParameterExpression filterParameter = Expression.Parameter(elementType, rangeVariable.Name);
            _lambdaParameters = new Dictionary<string, ParameterExpression>();
            _lambdaParameters.Add(rangeVariable.Name, filterParameter);

            EnsureFlattenedPropertyContainer(filterParameter);

            Expression body = Bind(expression);
            return Expression.Lambda(body, filterParameter);
        }

        private Expression ApplyNullPropagationForFilterBody(Expression body)
        {
            if (IsNullable(body.Type))
            {
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // handle null as false
                    // body => body == true. passing liftToNull:false would convert null to false.
                    body = Expression.Equal(body, Expression.Constant(true, typeof(bool?)), liftToNull: false, method: null);
                }
                else
                {
                    body = Expression.Convert(body, typeof(bool));
                }
            }

            return body;
        }

        /// <summary>
        /// Binds a <see cref="RangeVariable"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="RangeVariable"/>.
        /// </summary>
        /// <param name="rangeVariable">The range variable to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindRangeVariable(RangeVariable rangeVariable)
        {
            ParameterExpression parameter = _lambdaParameters[rangeVariable.Name];
            return ConvertNonStandardPrimitives(parameter);
        }

        /// <summary>
        /// Binds a <see cref="CollectionPropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionPropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode)
        {
            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="CollectionComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionComplexNode"/>.
        /// </summary>
        /// <param name="collectionComplexNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionComplexNode(CollectionComplexNode collectionComplexNode)
        {
            Expression source = Bind(collectionComplexNode.Source);
            return CreatePropertyAccessExpression(source, collectionComplexNode.Property);
        }

        /// <summary>
        /// Binds a <see cref="SingleValuePropertyAccessNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValuePropertyAccessNode"/>.
        /// </summary>
        /// <param name="propertyAccessNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindPropertyAccessQueryNode(SingleValuePropertyAccessNode propertyAccessNode)
        {
            Expression source = Bind(propertyAccessNode.Source);
            return CreatePropertyAccessExpression(source, propertyAccessNode.Property, GetFullPropertyPath(propertyAccessNode));
        }

        /// <summary>
        /// Binds a <see cref="SingleComplexNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleComplexNode"/>.
        /// </summary>
        /// <param name="singleComplexNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleComplexNode(SingleComplexNode singleComplexNode)
        {
            Expression source = Bind(singleComplexNode.Source);
            return CreatePropertyAccessExpression(source, singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property, string propertyPath = null)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, Model);
            propertyPath = propertyPath ?? propertyName;

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) && source != _lambdaParameters[ODataItParameterName])
            {
                var cleanSource = RemoveInnerNullPropagation(source);
                Expression propertyAccessExpression = null;

                propertyAccessExpression = GetFlattenedPropertyExpression(propertyPath) ?? Expression.Property(cleanSource, propertyName);

                // source.property => source == null ? null : [CastToNullable]RemoveInnerNullPropagation(source).property
                // Notice that we are checking if source is null already. so we can safely remove any null checks when doing source.Property

                Expression ifFalse = ToNullable(ConvertNonStandardPrimitives(propertyAccessExpression));
                return
                    Expression.Condition(
                        test: Expression.Equal(source, NullConstant),
                        ifTrue: Expression.Constant(null, ifFalse.Type),
                        ifFalse: ifFalse);
            }
            else
            {
                return GetFlattenedPropertyExpression(propertyPath) ?? ConvertNonStandardPrimitives(Expression.Property(source, propertyName));
            }
        }

        /// <summary>
        /// Binds a <see cref="UnaryOperatorNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="UnaryOperatorNode"/>.
        /// </summary>
        /// <param name="unaryOperatorNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode)
        {
            // No need to handle null-propagation here as CLR already handles it.
            // !(null) = null
            // -(null) = null
            Expression inner = Bind(unaryOperatorNode.Operand);
            switch (unaryOperatorNode.OperatorKind)
            {
                case UnaryOperatorKind.Negate:
                    return Expression.Negate(inner);

                case UnaryOperatorKind.Not:
                    return Expression.Not(inner);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, unaryOperatorNode.Kind, typeof(FilterBinder).Name);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            switch (node.Name)
            {
                case ClrCanonicalFunctions.StartswithFunctionName:
                    return BindStartsWith(node);

                case ClrCanonicalFunctions.EndswithFunctionName:
                    return BindEndsWith(node);

                case ClrCanonicalFunctions.ContainsFunctionName:
                    return BindContains(node);

                case ClrCanonicalFunctions.SubstringFunctionName:
                    return BindSubstring(node);

                case ClrCanonicalFunctions.LengthFunctionName:
                    return BindLength(node);

                case ClrCanonicalFunctions.IndexofFunctionName:
                    return BindIndexOf(node);

                case ClrCanonicalFunctions.TolowerFunctionName:
                    return BindToLower(node);

                case ClrCanonicalFunctions.ToupperFunctionName:
                    return BindToUpper(node);

                case ClrCanonicalFunctions.TrimFunctionName:
                    return BindTrim(node);

                case ClrCanonicalFunctions.ConcatFunctionName:
                    return BindConcat(node);

                case ClrCanonicalFunctions.YearFunctionName:
                case ClrCanonicalFunctions.MonthFunctionName:
                case ClrCanonicalFunctions.DayFunctionName:
                    return BindDateRelatedProperty(node); // Date & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.HourFunctionName:
                case ClrCanonicalFunctions.MinuteFunctionName:
                case ClrCanonicalFunctions.SecondFunctionName:
                    return BindTimeRelatedProperty(node); // TimeOfDay & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                    return BindFractionalSeconds(node);

                case ClrCanonicalFunctions.RoundFunctionName:
                    return BindRound(node);

                case ClrCanonicalFunctions.FloorFunctionName:
                    return BindFloor(node);

                case ClrCanonicalFunctions.CeilingFunctionName:
                    return BindCeiling(node);

                case ClrCanonicalFunctions.CastFunctionName:
                    return BindCastSingleValue(node);

                case ClrCanonicalFunctions.IsofFunctionName:
                    return BindIsOf(node);

                case ClrCanonicalFunctions.DateFunctionName:
                    return BindDate(node);

                case ClrCanonicalFunctions.TimeFunctionName:
                    return BindTime(node);

                default:
                    // Get Expression of custom binded method.
                    Expression expression = BindCustomMethodExpressionOrNull(node);
                    if (expression != null)
                    {
                        return expression;
                    }

                    throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
            }
        }

        private Expression BindCastSingleValue(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? _lambdaParameters[ODataItParameterName] : arguments[0];
            string targetTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = Model.FindType(targetTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                IEdmTypeReference targetEdmTypeReference = targetEdmType.ToEdmTypeReference(false);
                targetClrType = EdmLibHelpers.GetClrType(targetEdmTypeReference, Model);

                if (source != NullConstant)
                {
                    if (source.Type == targetClrType)
                    {
                        return source;
                    }

                    if ((!targetEdmTypeReference.IsPrimitive() && !targetEdmTypeReference.IsEnum()) ||
                        (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(source.Type) == null && !TypeHelper.IsEnum(source.Type)))
                    {
                        // Cast fails and return null.
                        return NullConstant;
                    }
                }
            }

            if (targetClrType == null || source == NullConstant)
            {
                return NullConstant;
            }

            if (targetClrType == typeof(string))
            {
                return BindCastToStringType(source);
            }
            else if (TypeHelper.IsEnum(targetClrType))
            {
                return BindCastToEnumType(source.Type, targetClrType, node.Parameters.First(), arguments.Length);
            }
            else
            {
                if (source.Type.IsNullable() && !targetClrType.IsNullable())
                {
                    // Make the target Clr type nullable to avoid failure while casting
                    // nullable source, whose value may be null, to a non-nullable type.
                    // For example: cast(NullableInt32Property,Edm.Int64)
                    // The target Clr type should be Nullable<Int64> rather than Int64.
                    targetClrType = typeof(Nullable<>).MakeGenericType(targetClrType);
                }

                try
                {
                    return Expression.Convert(source, targetClrType);
                }
                catch (InvalidOperationException)
                {
                    // Cast fails and return null.
                    return NullConstant;
                }
            }
        }

        private static Expression BindCastToStringType(Expression source)
        {
            Expression sourceValue;

            if (source.Type.IsGenericType && source.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // Entity Framework doesn't have ToString method for enum types.
                    // Convert enum types to their underlying numeric types.
                    sourceValue = Expression.Convert(
                        Expression.Property(source, "Value"),
                        Enum.GetUnderlyingType(TypeHelper.GetUnderlyingTypeOrSelf(source.Type)));
                }
                else
                {
                    // Entity Framework has ToString method for numeric types.
                    sourceValue = Expression.Property(source, "Value");
                }

                // Entity Framework doesn't have ToString method for nullable numeric types.
                // Call ToString method on non-nullable numeric types.
                return Expression.Condition(
                    Expression.Property(source, "HasValue"),
                    Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null),
                    Expression.Constant(null, typeof(string)));
            }
            else
            {
                sourceValue = TypeHelper.IsEnum(source.Type) ?
                    Expression.Convert(source, Enum.GetUnderlyingType(source.Type)) :
                    source;
                return Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null);
            }
        }

        private Expression BindCastToEnumType(Type sourceType, Type targetClrType, QueryNode firstParameter, int parameterLength)
        {
            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(targetClrType);
            ConstantNode sourceNode = firstParameter as ConstantNode;

            if (parameterLength == 1 || sourceNode == null || sourceType != typeof(string))
            {
                // We only support to cast Enumeration type from constant string now,
                // because LINQ to Entities does not recognize the method Enum.TryParse.
                return NullConstant;
            }
            else
            {
                object[] parameters = new[] { sourceNode.Value, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (isSuccessful)
                {
                    if (QuerySettings.EnableConstantParameterization)
                    {
                        return LinqParameterContainer.Parameterize(targetClrType, parameters[1]);
                    }
                    else
                    {
                        return Expression.Constant(parameters[1], targetClrType);
                    }
                }
                else
                {
                    return NullConstant;
                }
            }
        }

        private Expression BindIsOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.IsofFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // Edm.Boolean isof(type)  or
            // Edm.Boolean isof(expression,type)
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? _lambdaParameters[ODataItParameterName] : arguments[0];
            if (source == NullConstant)
            {
                return FalseConstant;
            }

            string typeName = (string)((ConstantNode)node.Parameters.Last()).Value;

            IEdmType edmType = Model.FindType(typeName);
            Type clrType = null;
            if (edmType != null)
            {
                // bool nullable = source.Type.IsNullable();
                IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(false);
                clrType = EdmLibHelpers.GetClrType(edmTypeReference, Model);
            }

            if (clrType == null)
            {
                return FalseConstant;
            }

            bool isSourcePrimitiveOrEnum = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(source.Type) != null ||
                                           TypeHelper.IsEnum(source.Type);

            bool isTargetPrimitiveOrEnum = EdmLibHelpers.GetEdmPrimitiveTypeOrNull(clrType) != null ||
                                           TypeHelper.IsEnum(clrType);

            if (isSourcePrimitiveOrEnum && isTargetPrimitiveOrEnum)
            {
                if (source.Type.IsNullable())
                {
                    clrType = clrType.ToNullable();
                }
            }

            // Be caution: Type method of LINQ to Entities only supports entity type.
            return Expression.Condition(Expression.TypeIs(source, clrType), TrueConstant, FalseConstant);
        }

        private Expression BindCeiling(SingleValueFunctionCallNode node)
        {
            Contract.Assert("ceiling" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo ceiling = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.CeilingOfDouble
                : ClrCanonicalFunctions.CeilingOfDecimal;
            return MakeFunctionCall(ceiling, arguments);
        }

        private Expression BindFloor(SingleValueFunctionCallNode node)
        {
            Contract.Assert("floor" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo floor = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.FloorOfDouble
                : ClrCanonicalFunctions.FloorOfDecimal;
            return MakeFunctionCall(floor, arguments);
        }

        private Expression BindRound(SingleValueFunctionCallNode node)
        {
            Contract.Assert("round" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo round = IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.RoundOfDouble
                : ClrCanonicalFunctions.RoundOfDecimal;
            return MakeFunctionCall(round, arguments);
        }

        private Expression BindDate(SingleValueFunctionCallNode node)
        {
            Contract.Assert("date" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new Date(int, int, int), also doesn't support other property access, for example DateTime.Date.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindTime(SingleValueFunctionCallNode node)
        {
            Contract.Assert("time" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new TimeOfDay(int, int, int, int), also doesn't support other property access, for example DateTimeOffset.DateTime.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindFractionalSeconds(SingleValueFunctionCallNode node)
        {
            Contract.Assert("fractionalseconds" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && (IsTimeRelated(arguments[0].Type)));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsTimeOfDay(parameter.Type))
            {
                property = ClrCanonicalFunctions.TimeOfDayProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else if (IsDateTime(parameter.Type))
            {
                property = ClrCanonicalFunctions.DateTimeProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else if (IsTimeSpan(parameter.Type))
            {
                property = ClrCanonicalFunctions.TimeSpanProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }
            else
            {
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[ClrCanonicalFunctions.MillisecondFunctionName];
            }

            // Millisecond
            Expression milliSecond = MakePropertyAccess(property, parameter);
            Expression decimalMilliSecond = Expression.Convert(milliSecond, typeof(decimal));
            Expression fractionalSeconds = Expression.Divide(decimalMilliSecond, Expression.Constant(1000m, typeof(decimal)));

            return CreateFunctionCallWithNullPropagation(fractionalSeconds, arguments);
        }

        private Expression BindDateRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && IsDateRelated(arguments[0].Type));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsDate(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateProperties[node.Name];
            }
            else if (IsDateTime(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
            }
            else
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
            }

            return MakeFunctionCall(property, parameter);
        }

        private Expression BindTimeRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && (IsTimeRelated(arguments[0].Type)));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (IsTimeOfDay(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.TimeOfDayProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.TimeOfDayProperties[node.Name];
            }
            else if (IsDateTime(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
            }
            else if (IsTimeSpan(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.TimeSpanProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.TimeSpanProperties[node.Name];
            }
            else
            {
                Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
            }

            return MakeFunctionCall(property, parameter);
        }

        private Expression BindConcat(SingleValueFunctionCallNode node)
        {
            Contract.Assert("concat" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Concat, arguments);
        }

        private Expression BindTrim(SingleValueFunctionCallNode node)
        {
            Contract.Assert("trim" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Trim, arguments);
        }

        private Expression BindToUpper(SingleValueFunctionCallNode node)
        {
            Contract.Assert("toupper" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.ToUpper, arguments);
        }

        private Expression BindToLower(SingleValueFunctionCallNode node)
        {
            Contract.Assert("tolower" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.ToLower, arguments);
        }

        private Expression BindIndexOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert("indexof" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.IndexOf, arguments);
        }

        private Expression BindSubstring(SingleValueFunctionCallNode node)
        {
            Contract.Assert("substring" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            if (arguments[0].Type != typeof(string))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, node.Name));
            }

            Expression functionCall;
            if (arguments.Length == 2)
            {
                Contract.Assert(IsInteger(arguments[1].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartNoThrow, arguments);
                }
                else
                {
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStart, arguments);
                }
            }
            else
            {
                // arguments.Length == 3 implies String.Substring(int, int)
                Contract.Assert(arguments.Length == 3 && IsInteger(arguments[1].Type) && IsInteger(arguments[2].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int, int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int, int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLengthNoThrow, arguments);
                }
                else
                {
                    functionCall = MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLength, arguments);
                }
            }

            return functionCall;
        }

        private Expression BindLength(SingleValueFunctionCallNode node)
        {
            Contract.Assert("length" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Length, arguments);
        }

        private Expression BindContains(SingleValueFunctionCallNode node)
        {
            Contract.Assert("contains" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.Contains, arguments[0], arguments[1]);
        }

        private Expression BindStartsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("startswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.StartsWith, arguments);
        }

        private Expression BindEndsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("endswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return MakeFunctionCall(ClrCanonicalFunctions.EndsWith, arguments);
        }

        private Expression[] BindArguments(IEnumerable<QueryNode> nodes)
        {
            return nodes.OfType<SingleValueNode>().Select(n => Bind(n)).ToArray();
        }

        private static void ValidateAllStringArguments(string functionName, Expression[] arguments)
        {
            if (arguments.Any(arg => arg.Type != typeof(string)))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, functionName));
            }
        }

        /// <summary>
        /// Binds a <see cref="AllNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="AllNode"/>.
        /// </summary>
        /// <param name="allNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAllNode(AllNode allNode)
        {
            ParameterExpression allIt = HandleLambdaParameters(allNode.RangeVariables);

            Expression source;
            Contract.Assert(allNode.Source != null);
            source = Bind(allNode.Source);

            Expression body = source;
            Contract.Assert(allNode.Body != null);

            body = Bind(allNode.Body);
            body = ApplyNullPropagationForFilterBody(body);
            body = Expression.Lambda(body, allIt);

            Expression all = All(source, body);

            ExitLamdbaScope();

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                all = ToNullable(all);
                return Expression.Condition(
                    test: Expression.Equal(source, NullConstant),
                    ifTrue: Expression.Constant(null, all.Type),
                    ifFalse: all);
            }
            else
            {
                return all;
            }
        }

        /// <summary>
        /// Binds a <see cref="AnyNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="AnyNode"/>.
        /// </summary>
        /// <param name="anyNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindAnyNode(AnyNode anyNode)
        {
            ParameterExpression anyIt = HandleLambdaParameters(anyNode.RangeVariables);

            Expression source;
            Contract.Assert(anyNode.Source != null);
            source = Bind(anyNode.Source);

            Expression body = null;
            // uri parser places an Constant node with value true for empty any() body
            if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
            {
                body = Bind(anyNode.Body);
                body = ApplyNullPropagationForFilterBody(body);
                body = Expression.Lambda(body, anyIt);
            }

            Expression any = Any(source, body);

            ExitLamdbaScope();

            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type))
            {
                // IFF(source == null) null; else Any(body);
                any = ToNullable(any);
                return Expression.Condition(
                    test: Expression.Equal(source, NullConstant),
                    ifTrue: Expression.Constant(null, any.Type),
                    ifFalse: any);
            }
            else
            {
                return any;
            }
        }

        private Expression BindCustomMethodExpressionOrNull(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            IEnumerable<Type> methodArgumentsType = arguments.Select(argument => argument.Type);

            // Search for custom method info that are binded to the node name
            MethodInfo methodInfo;
            if (UriFunctionsBinder.TryGetMethodInfo(node.Name, methodArgumentsType, out methodInfo))
            {
                return MakeFunctionCall(methodInfo, arguments);
            }

            return null;
        }

        private ParameterExpression HandleLambdaParameters(IEnumerable<RangeVariable> rangeVariables)
        {
            ParameterExpression lambdaIt = null;

            EnterLambdaScope();

            Dictionary<string, ParameterExpression> newParameters = new Dictionary<string, ParameterExpression>();
            foreach (RangeVariable rangeVariable in rangeVariables)
            {
                ParameterExpression parameter;
                if (!_lambdaParameters.TryGetValue(rangeVariable.Name, out parameter))
                {
                    // Work-around issue 481323 where UriParser yields a collection parameter type
                    // for primitive collections rather than the inner element type of the collection.
                    // Remove this block of code when 481323 is resolved.
                    IEdmTypeReference edmTypeReference = rangeVariable.TypeReference;
                    IEdmCollectionTypeReference collectionTypeReference = edmTypeReference as IEdmCollectionTypeReference;
                    if (collectionTypeReference != null)
                    {
                        IEdmCollectionType collectionType = collectionTypeReference.Definition as IEdmCollectionType;
                        if (collectionType != null)
                        {
                            edmTypeReference = collectionType.ElementType;
                        }
                    }

                    parameter = Expression.Parameter(EdmLibHelpers.GetClrType(edmTypeReference, Model, AssembliesResolver), rangeVariable.Name);
                    Contract.Assert(lambdaIt == null, "There can be only one parameter in an Any/All lambda");
                    lambdaIt = parameter;
                }
                newParameters.Add(rangeVariable.Name, parameter);
            }

            _lambdaParameters = newParameters;
            return lambdaIt;
        }

        private void EnterLambdaScope()
        {
            Contract.Assert(_lambdaParameters != null);
            _parametersStack.Push(_lambdaParameters);
        }

        private void ExitLamdbaScope()
        {
            if (_parametersStack.Count != 0)
            {
                _lambdaParameters = _parametersStack.Pop();
            }
            else
            {
                _lambdaParameters = null;
            }
        }

        private static Expression Any(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Type elementType;
            source.Type.IsCollection(out elementType);
            Contract.Assert(elementType != null);

            if (filter == null)
            {
                if (IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableEmptyAnyGeneric.MakeGenericMethod(elementType), source);
                }
            }
            else
            {
                if (IsIQueryable(source.Type))
                {
                    return Expression.Call(null, ExpressionHelperMethods.QueryableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
                else
                {
                    return Expression.Call(null, ExpressionHelperMethods.EnumerableNonEmptyAnyGeneric.MakeGenericMethod(elementType), source, filter);
                }
            }
        }

        private static Expression All(Expression source, Expression filter)
        {
            Contract.Assert(source != null);
            Contract.Assert(filter != null);

            Type elementType;
            source.Type.IsCollection(out elementType);
            Contract.Assert(elementType != null);

            if (IsIQueryable(source.Type))
            {
                return Expression.Call(null, ExpressionHelperMethods.QueryableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
            else
            {
                return Expression.Call(null, ExpressionHelperMethods.EnumerableAllGeneric.MakeGenericMethod(elementType), source, filter);
            }
        }
    }
}
