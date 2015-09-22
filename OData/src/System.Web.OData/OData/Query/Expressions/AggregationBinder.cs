using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;

namespace System.Web.OData.Query.Expressions
{
    internal class AggregationBinder : ExpressionBinderBase
    {
        private Type _elementType;
        private QueryNode _transformation;

        ParameterExpression _lambdaParameter;

        private IEnumerable<AggregateStatementNode2> _aggregateStatements;
        private IEnumerable<SingleValuePropertyAccessNode> _groupingProperties;

        private Type _groupByClrType;

        public AggregationBinder(ODataQuerySettings settings, IAssembliesResolver assembliesResolver, Type elementType, IEdmModel model, QueryNode transformation)
            : base(model, assembliesResolver, settings)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(transformation != null);

            _elementType = elementType;
            _transformation = transformation;

            this._lambdaParameter = Expression.Parameter(this._elementType);

            switch (transformation.Kind)
            {
                case QueryNodeKind.Aggregate:
                    var aggregateClause = this._transformation as AggregateNode2;
                    ResultType = aggregateClause.TypeReference;
                    _aggregateStatements = aggregateClause.Statements;
                    break;
                case QueryNodeKind.GroupBy:
                    var groupByClause = this._transformation as GroupByNode2;
                    ResultType = groupByClause.TypeReference;
                    _groupingProperties = groupByClause.GroupingProperties;
                    _aggregateStatements = groupByClause.Aggregate != null ? groupByClause.Aggregate.Statements : null;
                    _groupByClrType = TypeProvider.GetResultType(groupByClause.GroupingTypeReference, _model);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Not supported transformation kind {0}", transformation.Kind));
            }

            ResultClrType = TypeProvider.GetResultType(ResultType, _model);

            _groupByClrType = _groupByClrType ?? ResultClrType;
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
            ParameterExpression accum = Expression.Parameter(groupingType);

            List<MemberAssignment> wrapperTypeMemberAssignments2 = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (_groupingProperties != null)
            {
                foreach (var prop in _groupingProperties)
                {
                    wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultClrType.GetMember(prop.Property.Name).Single(), Expression.Property(Expression.Property(accum, "Key"), prop.Property.Name)));
                }
            }

            // Setting Container property when we have aggregation clauses
            if (_aggregateStatements != null)
            {
                foreach (var aggStatement in _aggregateStatements)
                {
                    wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultClrType.GetMember(aggStatement.AsAlias).Single(), CreateAggregationExpression(accum, aggStatement)));
                }
            }

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments2), accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateStatementNode2 statement)
        {
            LambdaExpression propertyLambda = Expression.Lambda(BindAccessor(statement.Expression), this._lambdaParameter);
            // I substitute the element type for all generic arguments.                                                
            var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(this._elementType);
            Expression asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);

            Expression aggregationExpression;

            switch (statement.WithVerb)
            {
                case AggregationVerb.Min:
                    {
                        var minMethod = ExpressionHelperMethods.QueryableMin.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationVerb.Max:
                    {
                        var maxMethod = ExpressionHelperMethods.QueryableMax.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, maxMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationVerb.Sum:
                    {
                        MethodInfo sumGenericMethod;
                        if (!ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyLambda.Body.Type, out sumGenericMethod))
                        {
                            throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", statement.WithVerb, statement.Expression, propertyLambda.Body.Type));
                        }
                        var sumMethod = sumGenericMethod.MakeGenericMethod(this._elementType);
                        aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationVerb.Average:
                    {
                        MethodInfo averageGenericMethod;
                        if (!ExpressionHelperMethods.QueryableAverageGenerics.TryGetValue(propertyLambda.Body.Type, out averageGenericMethod))
                        {
                            throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", statement.WithVerb, statement.Expression, propertyLambda.Body.Type));
                        }
                        var averageMethod = averageGenericMethod.MakeGenericMethod(this._elementType);
                        aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationVerb.CountDistinct:
                    {
                        // I select the specific field 
                        var selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQuerableExpression, propertyLambda);

                        // I run distinct over the set of items
                        var distinctMethod = ExpressionHelperMethods.QueryableDistinct.MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                        // I count the distinct items as the aggregation expression
                        var countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, countMethod, distinctExpression);
                    }
                    break;
                default:
                    throw new ODataException(Error.Format("Aggregation method '{0}' is not supported.", statement.WithVerb));
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
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source), propAccessNode.Property);
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = node as SingleNavigationNode;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source), navNode.NavigationProperty);
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = node as BinaryOperatorNode;
                    var leftExpression = BindAccessor(binaryNode.Left);
                    var rightExpression = BindAccessor(binaryNode.Right);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression, liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = node as ConvertNode;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source));
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(AggregationBinder).Name);
            }
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, _model);
            if (_querySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) && source != this._lambdaParameter)
            {
                Expression propertyAccessExpression = Expression.Property(RemoveInnerNullPropagation(source), propertyName);

                // source.property => source == null ? null : [CastToNullable]RemoveInnerNullPropagation(source).property
                // Notice that we are checking if source is null already. so we can safely remove any null checks when doing source.Property

                Expression ifFalse = ToNullable(ConvertNonStandardPrimitives(propertyAccessExpression));
                return
                    Expression.Condition(
                        test: Expression.Equal(source, _nullConstant),
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

                List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
                foreach (var node in _groupingProperties)
                {
                    wrapperTypeMemberAssignments.Add(Expression.Bind(_groupByClrType.GetMember(node.Property.Name).Single(), BindAccessor(node)));
                }

                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(this._groupByClrType), wrapperTypeMemberAssignments), this._lambdaParameter);
            }
            else
            {

                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(this._groupByClrType), this._lambdaParameter);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, this._groupByClrType);
        }
    }
}
