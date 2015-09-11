using Microsoft.OData.Core;
using Microsoft.OData.Core.Aggregation;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Core.UriParser.TreeNodeKinds;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.OData.Query.Expressions
{
    internal class AggregationBinder : ExpressionBinderBase
    {
        private Type _elementType;
        private AggregationTransformationBase _transformation;

        ParameterExpression _lambdaParameter;

        private ApplyAggregateClause _aggregateClause;
        private IList<SingleValuePropertyAccessNode> _selectedProperties;
        private bool _grouped = false;
        private Type _groupByWrapperType;
        private TypeDefinition _groupByTypeDef;


        public AggregationBinder(ODataQuerySettings settings, IAssembliesResolver assembliesResolver, Type elementType, IEdmModel model, AggregationTransformationBase transformation)
            : base(model, assembliesResolver, settings)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(transformation != null);

            _elementType = elementType;
            _transformation = transformation;

            // TODO: Do we need to use Range?
            this._lambdaParameter = Expression.Parameter(this._elementType);

            CreateQueryClauses();
            CreateGroupByType();
        }

        /// <summary>
        /// Gets CLR type returned from the query.
        /// </summary>
        public Type ResultType
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

        private void CreateQueryClauses()
        {
            _aggregateClause = this._transformation as ApplyAggregateClause;
            _grouped = false;
            if (_aggregateClause == null)
            {
                var groupByClause = this._transformation as ApplyGroupbyClause;
                if (groupByClause != null)
                {
                    // TODO: After refactoring of GroupByClause it will just have that property IList<SingleValuePropertyAccessNode>
                    _selectedProperties = groupByClause.SelectedPropertiesExpressions.Select(p => p.Expression).Cast<SingleValuePropertyAccessNode>().ToList();
                    _aggregateClause = groupByClause.Aggregate;
                    _grouped = true;
                }
                else
                {
                    throw new NotSupportedException(string.Format("Not supported transformation type {0}", this._transformation));
                }
            }
        }

        private void CreateGroupByType()
        {
            _groupByTypeDef = new TypeDefinition();
            // TODO: As soon we have IEdmType after parsing just use it
            if (_selectedProperties != null && _selectedProperties.Any())
            {
                foreach (var node in _selectedProperties)
                {
                    _groupByTypeDef.Properties.Add(node.Property.Name, EdmLibHelpers.GetClrType(node.Property.Type, _model));
                }
            }

            _groupByWrapperType = TypeProvider.GetResultType(_groupByTypeDef);
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

            var groupingType = typeof(IGrouping<,>).MakeGenericType(_groupByWrapperType, this._elementType);
            ParameterExpression accum = Expression.Parameter(groupingType);
            Expression aggregationExpression = CreateAggregationExpression(accum);

            var resultTypeDef = this._groupByTypeDef.Clone();
            if (_aggregateClause != null)
            {
                resultTypeDef.Properties.Add(_aggregateClause.Alias, aggregationExpression.Type);
            }

            // TODO: Move and initialize earlier as soon as we switch to EdmTypes build during parsing
            this.ResultType = TypeProvider.GetResultType(resultTypeDef);

            List<MemberAssignment> wrapperTypeMemberAssignments2 = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (_grouped)
            {

                foreach (var prop in _groupByTypeDef.Properties)
                {
                    wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultType.GetMember(prop.Key).Single(), Expression.Property(Expression.Property(accum, "Key"), prop.Key)));
                }
            }

            // Setting Container property when we have aggregation clauses
            if (_aggregateClause != null)
            {
                wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultType.GetMember(_aggregateClause.Alias).Single(), aggregationExpression));
            }

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(ResultType), wrapperTypeMemberAssignments2), accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }

        private Expression CreateAggregationExpression(ParameterExpression accum)
        {
            if (_aggregateClause != null)
            {
                LambdaExpression propertyLambda = Expression.Lambda(BindAccessor(_aggregateClause.AggregatablePropertyExpression.Expression), this._lambdaParameter);
                // I substitute the element type for all generic arguments.                                                
                var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(this._elementType);
                Expression asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);

                Expression aggregationExpression;

                switch (_aggregateClause.AggregationMethod)
                {
                    case "min":
                        {
                            var minMethod = ExpressionHelperMethods.QueryableMin.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                            aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                        }
                        break;
                    case "max":
                        {
                            var maxMethod = ExpressionHelperMethods.QueryableMax.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                            aggregationExpression = Expression.Call(null, maxMethod, asQuerableExpression, propertyLambda);
                        }
                        break;
                    case "sum":
                        {
                            MethodInfo sumGenericMethod;
                            if (!ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyLambda.Body.Type, out sumGenericMethod))
                            {
                                throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", _aggregateClause.AggregationMethod, _aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
                            }
                            var sumMethod = sumGenericMethod.MakeGenericMethod(this._elementType);
                            aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);
                        }
                        break;
                    case "average":
                        {
                            MethodInfo averageGenericMethod;
                            if (!ExpressionHelperMethods.QueryableAverageGenerics.TryGetValue(propertyLambda.Body.Type, out averageGenericMethod))
                            {
                                throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", _aggregateClause.AggregationMethod, _aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
                            }
                            var averageMethod = averageGenericMethod.MakeGenericMethod(this._elementType);
                            aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);
                        }
                        break;
                    case "countdistinct":
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
                        throw new ODataException(Error.Format("Aggregation method '{0}' is not supported.", _aggregateClause.AggregationMethod));
                }

                return aggregationExpression;
            }

            return null;
        }

        private Expression BindAccessor(SingleValueNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.EntityRangeVariableReference:
                    return this._lambdaParameter;
                // TODO: Add null checks
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
            if (_selectedProperties != null && _selectedProperties.Any())
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
                foreach (var node in _selectedProperties)
                {
                    wrapperTypeMemberAssignments.Add(Expression.Bind(_groupByWrapperType.GetMember(node.Property.Name).Single(), BindAccessor(node)));
                }

                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(_groupByWrapperType), wrapperTypeMemberAssignments), this._lambdaParameter);
            }
            else
            {

                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(_groupByWrapperType), this._lambdaParameter);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, _groupByWrapperType);
        }
    }
}
