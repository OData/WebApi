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
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Aggregation;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query.Expressions
{
    internal class AggregationBinder : ExpressionBinderBase
    {
        private Type _elementType;
        private TransformationNode _transformation;

        private ParameterExpression _lambdaParameter;

        private IEnumerable<AggregateExpression> _aggregateExpressions;
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
                    _aggregateExpressions = aggregateClause.Expressions;
                    ResultClrType = AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(_model, null,
                        _aggregateExpressions);
                    break;
                case TransformationNodeKind.GroupBy:
                    var groupByClause = this._transformation as GroupByTransformationNode;
                    _groupingProperties = groupByClause.GroupingProperties;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            _aggregateExpressions =
                                ((AggregateTransformationNode)groupByClause.ChildTransformations).Expressions;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    _groupByClrType = AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(_model,
                        _groupingProperties, null);
                    ResultClrType = AggregationDynamicTypeProvider.GetResultType<DynamicTypeWrapper>(_model,
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
            wrapperTypeMemberAssignments = CreateSelectMemberAssigments(ResultClrType, propertyAccessor,
                _groupingProperties);

            // Setting Container property when we have aggregation clauses
            if (_aggregateExpressions != null)
            {
                foreach (var aggExpression in _aggregateExpressions)
                {
                    wrapperTypeMemberAssignments.Add(
                        Expression.Bind(ResultClrType.GetMember(aggExpression.Alias).Single(),
                            CreateAggregationExpression(accum, aggExpression)));
                }
            }

            var selectLambda =
                Expression.Lambda(Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments),
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

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateExpression expression)
        {
            Expression propertyAccessor = BindAccessor(expression.Expression);
            LambdaExpression propertyLambda = Expression.Lambda(propertyAccessor,
                this._lambdaParameter);
            // I substitute the element type for all generic arguments.
            var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(this._elementType);
            Expression asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);

            Expression aggregationExpression;

            switch (expression.Method)
            {
                case AggregationMethod.Min:
                    {
                        var minMethod = ExpressionHelperMethods.QueryableMin.MakeGenericMethod(this._elementType,
                            propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Max:
                    {
                        var maxMethod = ExpressionHelperMethods.QueryableMax.MakeGenericMethod(this._elementType,
                            propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, maxMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Sum:
                    {
                        MethodInfo sumGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(propertyAccessor);
                        propertyLambda = Expression.Lambda(propertyExpression, this._lambdaParameter);

                        if (
                            !ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyExpression.Type,
                                out sumGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var sumMethod = sumGenericMethod.MakeGenericMethod(this._elementType);
                        aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object
                        if (propertyAccessor.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.Average:
                    {
                        MethodInfo averageGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(propertyAccessor);
                        propertyLambda = Expression.Lambda(propertyExpression, this._lambdaParameter);

                        if (
                            !ExpressionHelperMethods.QueryableAverageGenerics.TryGetValue(propertyExpression.Type,
                                out averageGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var averageMethod = averageGenericMethod.MakeGenericMethod(this._elementType);
                        aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object 
                        if (propertyAccessor.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.CountDistinct:
                    {
                        // I select the specific field
                        var selectMethod =
                            ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(this._elementType,
                                propertyLambda.Body.Type);
                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQuerableExpression,
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

        private Expression BindAccessor(SingleValueNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.EntityRangeVariableReference:
                    return this._lambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = (SingleValuePropertyAccessNode)node;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source), propAccessNode.Property);
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = (SingleValueOpenPropertyAccessNode)node;
                    return CreateOpenPropertyAccessExpression(openNode);
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = (SingleNavigationNode)node;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source), navNode.NavigationProperty);
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = (BinaryOperatorNode)node;
                    var leftExpression = BindAccessor(binaryNode.Left);
                    var rightExpression = BindAccessor(binaryNode.Right);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = (ConvertNode)node;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source));
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind,
                        typeof(AggregationBinder).Name);
            }
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, _model);
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) &&
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

        private Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode)
        {
            var sourceAccessor = BindAccessor(openNode.Source);

            // First check that property exists in source
            // It's the case when we are apply transformation based on earlier transformation
            if (sourceAccessor.Type.GetProperty(openNode.Name) != null)
            {
                return Expression.Property(sourceAccessor, openNode.Name);
            }

            // Property doesn't exists go for dynamic properties dictionary
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);
            var propertyAccessExpression = Expression.Property(sourceAccessor, prop.Name);
            var readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                            DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            var containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            var nullExpression = Expression.Constant(null);

            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
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

        private static Expression WrapDynamicCastIfNeeded(Expression propertyAccessor)
        {
            if (propertyAccessor.Type == typeof(object))
            {
                return Expression.Call(null, ExpressionHelperMethods.ConvertToDecimal, propertyAccessor);
            }

            return propertyAccessor;
        }
    }
}
