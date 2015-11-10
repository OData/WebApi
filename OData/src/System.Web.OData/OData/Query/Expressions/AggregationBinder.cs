using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
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

        private IEnumerable<AggregateStatementNode> _aggregateStatements;
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
                    var aggregateClause = this._transformation as AggregateNode;
                    ResultType = aggregateClause.ItemType;
                    _aggregateStatements = aggregateClause.Statements;
                    break;
                case QueryNodeKind.GroupBy:
                    var groupByClause = this._transformation as GroupByNode;
                    ResultType = groupByClause.ItemType;
                    _groupingProperties = groupByClause.GroupingProperties;
                    _aggregateStatements = groupByClause.Aggregate != null ? groupByClause.Aggregate.Statements : null;
                    _groupByClrType = TypeProvider.GetResultType<DynamicEntityWrapper>(groupByClause.GroupingItemType, _model);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Not supported transformation kind {0}", transformation.Kind));
            }

            ResultClrType = TypeProvider.GetResultType<DynamicEntityWrapper>(ResultType, _model);

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
            ParameterExpression accum = Expression.Parameter(groupingType);

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (_groupingProperties != null)
            {
                foreach (var node in _groupingProperties)
                {
                    var stack = ReverseAccessNode(node);
                    var propertyAccessor = Expression.Property(accum, "Key");
                    while (stack.Count != 0)
                    {
                        var propNode = stack.Pop();
                        propertyAccessor = Expression.Property(propertyAccessor, GetNodePropertyName(propNode));
                    }
                    stack = ReverseAccessNode(node);
                    var prop = stack.Pop();
                    var member = ResultClrType.GetMember(GetNodePropertyName(prop)).Single();
                    if (stack.Count == 0)
                    {
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, propertyAccessor));
                    }
                    else
                    {
                        // TODO: Do proper recursion
                        var wrapperTypeMemberAssignments2 = new List<MemberAssignment>();
                        var membetType = (member as PropertyInfo).PropertyType;
                        var prop2 = stack.Pop();
                        var member2 = membetType.GetMember(GetNodePropertyName(prop2)).Single();
                        wrapperTypeMemberAssignments2.Add(Expression.Bind(member2, propertyAccessor));
                        var expr = Expression.MemberInit(Expression.New(membetType), wrapperTypeMemberAssignments2);
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, expr));
                    }

                    //wrapperTypeMemberAssignments.Add(Expression.Bind(ResultClrType.GetMember(node.Property.Name).Single(), propertyAccessor));
                }
            }

            // Setting Container property when we have aggregation clauses
            if (_aggregateStatements != null)
            {
                foreach (var aggStatement in _aggregateStatements)
                {
                    wrapperTypeMemberAssignments.Add(Expression.Bind(ResultClrType.GetMember(aggStatement.AsAlias).Single(), CreateAggregationExpression(accum, aggStatement)));
                }
            }

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments), accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateStatementNode statement)
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
                    var stack = ReverseAccessNode(node);
                    var prop = stack.Pop();
                    var member = _groupByClrType.GetMember(GetNodePropertyName(prop)).Single();
                    var nodeAccessor = BindAccessor(node);
                    if (stack.Count == 0)
                    {
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, nodeAccessor));
                    }
                    else
                    {
                        // TODO: Do proper recursion
                        var wrapperTypeMemberAssignments2 = new List<MemberAssignment>();
                        var membetType = (member as PropertyInfo).PropertyType;
                        var prop2 = stack.Pop();
                        var member2 = membetType.GetMember(GetNodePropertyName(prop2)).Single();
                        wrapperTypeMemberAssignments2.Add(Expression.Bind(member2, nodeAccessor));
                        var expr = Expression.MemberInit(Expression.New(membetType), wrapperTypeMemberAssignments2);
                        wrapperTypeMemberAssignments.Add(Expression.Bind(member, expr));
                    }
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

        // TODO: Find good extension class to land that method
        private Stack<SingleValueNode> ReverseAccessNode(SingleValueNode node)
        {
            var result = new Stack<SingleValueNode>();
            do
            {
                result.Push(node);
                if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
                {
                    node = ((SingleValuePropertyAccessNode)node).Source;

                }
                else if (node.Kind == QueryNodeKind.SingleNavigationNode)
                {
                    node = ((SingleNavigationNode)node).NavigationSource as SingleValueNode;
                }
            } while (node != null && (node.Kind == QueryNodeKind.SingleValuePropertyAccess || node.Kind == QueryNodeKind.SingleNavigationNode));

            return result;
        }

        private static string GetNodePropertyName(SingleValueNode property)
        {
            string propertyName = null;
            if (property.Kind == QueryNodeKind.SingleValuePropertyAccess)
            {
                propertyName = ((SingleValuePropertyAccessNode)property).Property.Name;
            }
            else if (property.Kind == QueryNodeKind.SingleNavigationNode)
            {
                propertyName = ((SingleNavigationNode)property).NavigationProperty.Name;
            }

            else
            {
                // TODO: Throw?
            }

            return propertyName;
        }
    }
}
