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

        private IEnumerable<AggregateExpression> _aggregateExpressions;
        private IEnumerable<GroupByPropertyNode> _groupingProperties;

        private Type _groupByClrType;
        
        private const string Linq2ObjectsQueryProviderNamespace = "System.Linq";

        private bool _linqToObjectMode = false;

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
                    ResultClrType = typeof(AggregationWrapper);
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

                    _groupByClrType = typeof(DynamicTypeWrapper);
                    ResultClrType = typeof(AggregationWrapper);
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
            this._linqToObjectMode = query.Provider.GetType().Namespace == Linq2ObjectsQueryProviderNamespace;

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

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            var propertyAccessor = Expression.Property(accum, "Key");
            var wrapperProperty2 = this.ResultClrType.GetProperty("GroupByContainer");

            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty2, Expression.Property(Expression.Property(accum, "Key"), "GroupByContainer")));

            //wrapperTypeMemberAssignments = CreateSelectMemberAssigments(ResultClrType, propertyAccessor,
            //    _groupingProperties);

            // Setting Container property when we have aggregation clauses
            if (_aggregateExpressions != null)
            {
                var properties = new List<NamedPropertyExpression>();
                foreach (var aggExpression in _aggregateExpressions)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(accum, aggExpression)));
                    //wrapperTypeMemberAssignments.Add(
                    //    Expression.Bind(ResultClrType.GetMember(aggExpression.Alias).Single(),
                    //        CreateAggregationExpression(accum, aggExpression)));
                }

                var wrapperProperty = ResultClrType.GetProperty("Container");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, PropertyContainer.CreateNextNamedPropertyContainer(properties)));
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
            LambdaExpression propertyLambda = Expression.Lambda(BindAccessor(expression.Expression),
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
                    if (
                        !ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyLambda.Body.Type,
                            out sumGenericMethod))
                    {
                        throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                            expression.Method, expression.Expression, propertyLambda.Body.Type));
                    }
                    var sumMethod = sumGenericMethod.MakeGenericMethod(this._elementType);
                    aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);
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
                    var averageMethod = averageGenericMethod.MakeGenericMethod(this._elementType);
                    aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);
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

            return this._linqToObjectMode 
                ? Expression.Convert(aggregationExpression, typeof(object))
                : aggregationExpression;
        }

        private Expression BindAccessor(SingleValueNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return this._lambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source), propAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    var singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source), singleComplexNode.Property);
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = node as SingleValueOpenPropertyAccessNode;
                    return Expression.Property(BindAccessor(openNode.Source), openNode.Name);
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = node as SingleNavigationNode;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source), navNode.NavigationProperty);
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = node as BinaryOperatorNode;
                    var leftExpression = BindAccessor(binaryNode.Left);
                    var rightExpression = BindAccessor(binaryNode.Right);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = node as ConvertNode;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source));
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
                List<NamedPropertyExpression> properties = CreateGroupByMemberAssignments2(_groupingProperties);

                
                var wrapperProperty = typeof(DynamicTypeWrapper).GetProperty("GroupByContainer");
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(wrapperProperty, PropertyContainer.CreateNextNamedPropertyContainer(properties)));
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(typeof(DynamicTypeWrapper)), wta), _lambdaParameter);
            }
            else
            {
                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(this._groupByClrType), this._lambdaParameter);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, this._groupByClrType);
        }

        private List<NamedPropertyExpression> CreateGroupByMemberAssignments2(IEnumerable<GroupByPropertyNode> nodes)
        {
            var properties = new List<NamedPropertyExpression>();
            foreach (var gProp in nodes)
            {
                var propertyName = gProp.Name;
                if (gProp.Expression != null)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), BindAccessor(gProp.Expression)));
                }
                else
                {
                    var wrapperProperty = typeof(NestedWrapper).GetProperty("NestedContainer");
                    List<MemberAssignment> wta = new List<MemberAssignment>();
                    wta.Add(Expression.Bind(wrapperProperty, PropertyContainer.CreateNextNamedPropertyContainer(CreateGroupByMemberAssignments2(gProp.ChildTransformations))));
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), Expression.MemberInit(Expression.New(typeof(NestedWrapper)), wta)));
                }
            }

            return properties;
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
