// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace System.Web.OData.Query.Expressions
{
    internal class AggregationBinder : ExpressionBinderBase
    {
        private Type _elementType;
        private TransformationNode _transformation;

        private ParameterExpression _lambdaParameter;

        private IEnumerable<AggregateExpressionBase> _aggregateExpressions;
        private IEnumerable<GroupByPropertyNode> _groupingProperties;

        private Type _groupByClrType;

        internal AggregationBinder(ODataQuerySettings settings, IAssembliesResolver assembliesResolver, Type elementType,
            IEdmModel model, TransformationNode transformation)
            : base(model, assembliesResolver, settings)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(transformation != null);

            _elementType = elementType;
            _transformation = transformation;

            this._lambdaParameter = Expression.Parameter(this._elementType, "$it");

            switch (transformation.Kind)
            {
                case TransformationNodeKind.Aggregate:
                    var aggregateClause = this._transformation as AggregateTransformationNode;
                    _aggregateExpressions = aggregateClause.AggregateExpressions;
                    ResultClrType = 
                        AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(Model, null, _aggregateExpressions);
                    break;
                case TransformationNodeKind.GroupBy:
                    var groupByClause = this._transformation as GroupByTransformationNode;
                    _groupingProperties = groupByClause.GroupingProperties;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            _aggregateExpressions =
                                ((AggregateTransformationNode)groupByClause.ChildTransformations).AggregateExpressions;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    _groupByClrType = AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(Model,
                        _groupingProperties, null);
                    ResultClrType = AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(Model,
                        _groupingProperties, _aggregateExpressions);
                    break;
                default:
                    throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                        SRResources.NotSupportedTransformationKind, transformation.Kind));
            }

            _groupByClrType = _groupByClrType ?? typeof(DynamicTypeWrapper);
        }

        /// <summary>
        /// Gets CLR type returned from the query.
        /// </summary>
        public Type ResultClrType
        {
            get; private set;
        }

        public IEdmTypeReference ResultType
        {
            get; private set;
        }

        public IQueryable Bind(IQueryable query)
        {
            // Answer is query.GroupBy($it => new DynamicType1() {...}).Select($it => new DynamicType2() {...})
            // We are doing Grouping even if only aggregate was specified to have a IQuaryable after aggregation
            IQueryable grouping = BindGroupBy(query);

            IQueryable result = BindSelect(grouping);

            return result;
        }

        private IQueryable BindSelect(IQueryable grouping)
        {
            // Should return following expression
            // .Select($it => New DynamicType2() 
            //                  {
            //                      Prop1 = $it.Prop1,
            //                      Prop2 = $it.Prop2,
            //                      Prop3 = $it.NavProp.Prop3
            //                      ...
            //                      Alias1 = $it.AsQuaryable().Sum(i => i.AggregatableProperty)
            //                  })

            var groupingType = typeof(IGrouping<,>).MakeGenericType(this._groupByClrType, this._elementType);
            ParameterExpression accum = Expression.Parameter(groupingType, "$it");

            List<MemberAssignment> wrapperTypeMemberAssignments = null;

            // Setting GroupByContainer property when previous step was grouping
            var propertyAccessor = Expression.Property(accum, "Key");
            wrapperTypeMemberAssignments = CreateSelectMemberAssigments(
                ResultClrType, propertyAccessor, _groupingProperties);

            // Setting Container property when we have aggregation clauses
            if (_aggregateExpressions != null)
            {
                foreach (var aggExpression in _aggregateExpressions)
                {
                    var member = ResultClrType.GetMember(aggExpression.Alias).Single() as PropertyInfo;
                    var value = CreateAggregationExpression(accum, aggExpression, member.PropertyType, this._elementType);
                    wrapperTypeMemberAssignments.Add(Expression.Bind(member, value));
                }
            }

            var selectLambda =
                Expression.Lambda(
                    Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments),
                    accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }

        private List<MemberAssignment> CreateSelectMemberAssigments(Type type, MemberExpression propertyAccessor,
            IEnumerable<GroupByPropertyNode> properties)
        {
            var wrapperTypeMemberAssignments = new List<MemberAssignment>();
            if (_groupingProperties != null)
            {
                foreach (var node in properties)
                {
                    var nodePropertyAccessor = Expression.Property(propertyAccessor, node.Name);
                    var member = type.GetMember(node.Name).Single();
                    if (node.Expression != null)
                    {
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, nodePropertyAccessor));
                    }
                    else
                    {
                        var memberType = (member as PropertyInfo).PropertyType;
                        var expr = Expression.MemberInit(Expression.New(memberType),
                            CreateSelectMemberAssigments(memberType, nodePropertyAccessor, node.ChildTransformations));
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, expr));
                    }
                }
            }

            return wrapperTypeMemberAssignments;
        }

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateExpressionBase expression, Type expressionType, Type baseType)
        {
            switch (expression.AggregateType)
            {
                case AggregateExpressionType.PropertyAggregate:
                    return CreatePropertyAggregateExpression(accum, expression as AggregateExpression, baseType);
                case AggregateExpressionType.EntitySetAggregate:
                    return CreateEntitySetAggregateExpression(accum, expression as EntitySetAggregateExpression, expressionType, baseType);
                default:
                    throw new ODataException("This type of aggregation is not supported.");
            }
        }

        private Expression CreateEntitySetAggregateExpression(
            ParameterExpression accum, EntitySetAggregateExpression expression, Type expressionType, Type baseType)
        {
            // Should return following expression
            //  $it => $it.AsQueryable()
            //      .SelectMany($it => $it.SomeEntitySet)
            //      .GroupBy($gr => new Object())
            //      .Select($p => new DynamicTypeWrapper()
            //      {
            //          AliasOne = $p.AsQueryable().AggMethodOne($it => $it.SomePropertyOfSomeEntitySet),
            //          AliasTwo = $p.AsQueryable().AggMethodTwo($it => $it.AnotherPropertyOfSomeEntitySet),
            //          ...
            //          AliasN =  ... , // A nested expression of this same format.
            //          ...
            //      })

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
            var asQueryableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(baseType);
            Expression asQueryableExpression = Expression.Call(null, asQueryableMethod, accum);

            // Create lambda to access the entity set from expression
            var source = BindAccessor(expression.Expression.Source);
            string propertyName = EdmLibHelpers.GetClrPropertyName(expression.Expression.NavigationProperty, Model);

            var property = Expression.Property(source, propertyName);

            var baseElementType = source.Type;
            var selectedElementType = property.Type.GenericTypeArguments.Single();

            // Create method to get property collections to aggregate
            MethodInfo selectManyMethod 
                = ExpressionHelperMethods.EnumerableSelectManyGeneric.MakeGenericMethod(baseElementType, selectedElementType);

            // Create the lambda that acceses the property in the selectMany clause.
            var selectManyParam = Expression.Parameter(baseElementType, "$it");
            var propertyExpression = Expression.Property(selectManyParam, expression.Expression.NavigationProperty.Name);
            var selectManyLambda = Expression.Lambda(propertyExpression, selectManyParam);

            // Get expression to get collection of entities
            var entitySet = Expression.Call(null, selectManyMethod, asQueryableExpression, selectManyLambda);

            // Getting method and lambda expression of groupBy
            var groupKeyType = typeof(object);
            MethodInfo groupByMethod = 
                ExpressionHelperMethods.EnumerableGroupByGeneric.MakeGenericMethod(selectedElementType, groupKeyType);
            var groupByLambda = Expression.Lambda(
                Expression.New(groupKeyType), 
                Expression.Parameter(selectedElementType, "$gr"));

            // Group entities in a single group to apply select
            var groupedEntitySet = Expression.Call(null, groupByMethod, entitySet, groupByLambda);

            var groupingType = typeof(IGrouping<,>).MakeGenericType(groupKeyType, selectedElementType);
            ParameterExpression innerAccum = Expression.Parameter(groupingType, "$p");

            // Create dynamicTypeWrapper to encapsulate the aggregate result
            var dynamicTypeWrapper = expressionType.GetGenericArguments().Single();
            foreach (var chidrenAgg in expression.Children)
            {
                var member = dynamicTypeWrapper.GetMember(chidrenAgg.Alias).Single();
                var aggExpression = CreateAggregationExpression(innerAccum, chidrenAgg, member.DeclaringType, selectedElementType);
                wrapperTypeMemberAssignments.Add(Expression.Bind(member, aggExpression));
            }

            // Get lambda to apply aggregations on collection of entities
            var selectLambda =
                Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(dynamicTypeWrapper), 
                        wrapperTypeMemberAssignments), 
                    innerAccum);

            // Get select method
            MethodInfo selectMethod = 
                ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                    groupingType,
                    dynamicTypeWrapper);

            // Apply select to get aggregated data struct
            return Expression.Call(null, selectMethod, groupedEntitySet, selectLambda);
        }

        private Expression CreatePropertyAggregateExpression(ParameterExpression accum, AggregateExpression expression, Type baseType)
        {
            var lambdaParameter = baseType == this._elementType ? this._lambdaParameter : Expression.Parameter(baseType, "$it");
            LambdaExpression propertyLambda = Expression.Lambda(BindAccessor(expression.Expression, lambdaParameter), lambdaParameter);

            // I substitute the element type for all generic arguments.                                                
            var asQueryableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(baseType);
            Expression asQueryableExpression = Expression.Call(null, asQueryableMethod, accum);

            Expression aggregationExpression;

            switch (expression.Method)
            {
                case AggregationMethod.Min:
                {
                    var minMethod = ExpressionHelperMethods.QueryableMin.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                    aggregationExpression = Expression.Call(null, minMethod, asQueryableExpression, propertyLambda);
                }
                    break;
                case AggregationMethod.Max:
                {
                    var maxMethod = ExpressionHelperMethods.QueryableMax.MakeGenericMethod(baseType, propertyLambda.Body.Type);
                    aggregationExpression = Expression.Call(null, maxMethod, asQueryableExpression, propertyLambda);
                }
                    break;
                case AggregationMethod.Sum:
                {
                    MethodInfo sumGenericMethod;
                    if (
                        !ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyLambda.Body.Type,
                            out sumGenericMethod))
                    {
                        throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                            expression.Method, expression.Expression, propertyLambda.Body.Type));
                    }
                    var sumMethod = sumGenericMethod.MakeGenericMethod(baseType);
                    aggregationExpression = Expression.Call(null, sumMethod, asQueryableExpression, propertyLambda);
                }
                    break;
                case AggregationMethod.Average:
                {
                    MethodInfo averageGenericMethod;
                    if (
                        !ExpressionHelperMethods.QueryableAverageGenerics.TryGetValue(propertyLambda.Body.Type,
                            out averageGenericMethod))
                    {
                        throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                            expression.Method, expression.Expression, propertyLambda.Body.Type));
                    }
                    var averageMethod = averageGenericMethod.MakeGenericMethod(baseType);
                    aggregationExpression = Expression.Call(null, averageMethod, asQueryableExpression, propertyLambda);
                }
                    break;
                case AggregationMethod.CountDistinct:
                {
                    // I select the specific field 
                    var selectMethod =
                        ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(baseType,
                            propertyLambda.Body.Type);
                    Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQueryableExpression,
                        propertyLambda);

                    // I run distinct over the set of items
                    var distinctMethod =
                        ExpressionHelperMethods.QueryableDistinct.MakeGenericMethod(propertyLambda.Body.Type);
                    Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                    // I count the distinct items as the aggregation expression
                    var countMethod =
                        ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(propertyLambda.Body.Type);
                    aggregationExpression = Expression.Call(null, countMethod, distinctExpression);
                }
                    break;
                default:
                    throw new ODataException(Error.Format(SRResources.AggregationMethodNotSupported, expression.Method));
            }

            return aggregationExpression;
        }

        private Expression BindAccessor(QueryNode node, Expression baseElement = null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return baseElement ?? this._lambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source, baseElement), propAccessNode.Property);
                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    var aggPropAccessNode = node as AggregatedCollectionPropertyNode;
                    return CreatePropertyAccessExpression(BindAccessor(aggPropAccessNode.Source, baseElement), aggPropAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    var singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source, baseElement), singleComplexNode.Property);
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = node as SingleValueOpenPropertyAccessNode;
                    return Expression.Property(BindAccessor(openNode.Source, baseElement), openNode.Name);
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = node as SingleNavigationNode;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source, baseElement), navNode.NavigationProperty);
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = node as BinaryOperatorNode;
                    var leftExpression = BindAccessor(binaryNode.Left, baseElement);
                    var rightExpression = BindAccessor(binaryNode.Right, baseElement);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = node as ConvertNode;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source, baseElement));
                case QueryNodeKind.CollectionNavigationNode:
                    return baseElement ?? this._lambdaParameter;
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind,
                        typeof(AggregationBinder).Name);
            }
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, Model);
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) &&
                source != this._lambdaParameter)
            {
                Expression propertyAccessExpression = Expression.Property(RemoveInnerNullPropagation(source),
                    propertyName);

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
                return ConvertNonStandardPrimitives(Expression.Property(source, propertyName));
            }
        }

        private IQueryable BindGroupBy(IQueryable query)
        {
            LambdaExpression groupLambda = null;
            if (_groupingProperties != null && _groupingProperties.Any())
            {
                // Generates expression
                // .GroupBy($it => new DynamicType1()
                //                                      {
                //                                          Prop1 = $it.Prop1,
                //                                          Prop2 = $it.Prop2,
                //                                          Prop3 = $it.NavProp.Prop3
                //                                          ...
                //                                      }) 

                List<MemberAssignment> wrapperTypeMemberAssignments = CreateGroupByMemberAssignments(_groupByClrType,
                    _groupingProperties);

                groupLambda =
                    Expression.Lambda(
                        Expression.MemberInit(Expression.New(this._groupByClrType), wrapperTypeMemberAssignments),
                        this._lambdaParameter);
            }
            else
            {
                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(this._groupByClrType), this._lambdaParameter);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, this._groupByClrType);
        }

        private List<MemberAssignment> CreateGroupByMemberAssignments(Type type,
            IEnumerable<GroupByPropertyNode> properties)
        {
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
            foreach (var node in properties)
            {
                var member = type.GetMember(node.Name).Single();

                if (node.Expression != null)
                {
                    wrapperTypeMemberAssignments.Add(Expression.Bind(member, BindAccessor(node.Expression)));
                }
                else
                {
                    var memberType = (member as PropertyInfo).PropertyType;
                    var expr = Expression.MemberInit(Expression.New(memberType),
                        CreateGroupByMemberAssignments(memberType, node.ChildTransformations));
                    wrapperTypeMemberAssignments.Add(Expression.Bind(member, expr));
                }
            }

            return wrapperTypeMemberAssignments;
        }
    }
}
