using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;
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
using System.Web.OData.Query;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.OData.Query.Expressions
{
    internal class AggregationBinder
    {
        private ODataQuerySettings _settings;
        private IAssembliesResolver _assembliesResolver;
        private Type _elementType;
        private AggregationTransformationBase _transformation;

        private ApplyAggregateClause aggregateClause;
        private IEnumerable<string> selectedStatements;
        private bool grouped = false;
        private Type GroupByWrapperType;
        private TypeDefinition groupByTypeDef;


        public AggregationBinder(ODataQuerySettings settings, IAssembliesResolver assembliesResolver, Type elementType, AggregationTransformationBase transformation)
        {
            Contract.Assert(settings != null);
            Contract.Assert(assembliesResolver != null);
            Contract.Assert(elementType != null);
            Contract.Assert(transformation != null);

            _settings = settings;
            _assembliesResolver = assembliesResolver;
            _elementType = elementType;
            _transformation = transformation;

            Init();
        }

        internal Type ResultType
        {
            get; private set;
        }

        public IQueryable Bind(IQueryable query)
        {
            // Answer is query.GroupBy($it => new DynamicType1() {...}).Select(GroupBy($it => new DynamicType1() {...}))
            // We are doing Grouping even if only aggregate was specified to have a IQuaryable after aggregation
            IQueryable grouping = BindGroupBy(query);

            IQueryable result = BindSelect(grouping);

            return result;
        }

        private void Init()
        {
            aggregateClause = this._transformation as ApplyAggregateClause;
            grouped = false;
            selectedStatements = null;
            if (aggregateClause == null)
            {
                var groupByClause = this._transformation as ApplyGroupbyClause;
                if (groupByClause != null)
                {
                    selectedStatements = groupByClause.SelectedStatements;
                    aggregateClause = groupByClause.Aggregate;
                    grouped = true;
                }
                else
                {
                    throw new NotSupportedException(string.Format("Not supported transformation type {0}", this._transformation));
                }
            }

            groupByTypeDef = new TypeDefinition();
            ParameterExpression source = Expression.Parameter(this._elementType);
            if (selectedStatements != null && selectedStatements.Any())
            {
                foreach (var propName in selectedStatements)
                {
                    var propertyName = propName.Trim();
                    groupByTypeDef.Properties.Add(propertyName, Expression.Property(source, propertyName).Type);
                }
            }

            GroupByWrapperType = TypeProvider.GetResultType(groupByTypeDef);
        }

        private IQueryable BindSelect(IQueryable grouping)
        {
            // Should return following expression
            // .Select($it => New DynamicType2() 
            //                  {
            //                      Prop1 = $it.Prop1,
            //                      Prop2 = $it.Prop2,
            //                      ...
            //                      Alias1 = $it.AsQuaryable().Sum(i => i.AggregatableProperty)
            //                  })

            var groupingType = typeof(IGrouping<,>).MakeGenericType(GroupByWrapperType, this._elementType);
            ParameterExpression accum = Expression.Parameter(groupingType);
            Expression aggregationExpression = CreateAggregationLambda(accum);

            var resultTypeDef = this.groupByTypeDef.Clone();
            if (aggregateClause != null)
            {
                resultTypeDef.Properties.Add(aggregateClause.Alias, aggregationExpression.Type);
            }

            // TODO: Move and initialize earlier as soon as we switch to EdmTypes build during parsing
            this.ResultType = TypeProvider.GetResultType(resultTypeDef);

            var keyProperty = ExpressionHelpers.GetPropertyAccessLambda(groupingType, "Key");

            List<MemberAssignment> wrapperTypeMemberAssignments2 = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (grouped)
            {
                
                foreach (var prop in groupByTypeDef.Properties)
                {
                    wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultType.GetMember(prop.Key).Single(), Expression.Property(Expression.Property(accum, "Key"), prop.Key)));
                }
            }

            // Setting Container property when we have aggregation clauses
            if (aggregateClause != null)
            {
                wrapperTypeMemberAssignments2.Add(Expression.Bind(ResultType.GetMember(aggregateClause.Alias).Single(), aggregationExpression));
            }

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(ResultType), wrapperTypeMemberAssignments2), accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }

        private Expression CreateAggregationLambda(ParameterExpression accum)
        {
            if (aggregateClause != null)
            {
                LambdaExpression propertyLambda = ExpressionHelpers.GetPropertyAccessLambda(this._elementType, aggregateClause.AggregatableProperty);
                // I substitute the element type for all generic arguments.                                                
                var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(this._elementType);
                Expression asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);

                Expression aggregationExpression;

                switch (aggregateClause.AggregationMethod)
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
                                throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", aggregateClause.AggregationMethod, aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
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
                                throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", aggregateClause.AggregationMethod, aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
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
                        throw new ODataException(Error.Format("Aggregation method '{0}' is not supported.", aggregateClause.AggregationMethod));
                }

                return aggregationExpression;
            }

            return null;
        }

        private IQueryable BindGroupBy(IQueryable query)
        {

            ParameterExpression source = Expression.Parameter(this._elementType);

            ConstructorInfo wrapperConstructor = GroupByWrapperType.GetConstructor(new Type[] { });
            NewExpression newExpression = Expression.New(wrapperConstructor);
            LambdaExpression groupLambda = null;
            if (selectedStatements != null && selectedStatements.Any())
            {
                // Generates expression
                // .GroupBy($it => new DynamicType1()
                //                                      {
                //                                          Prop1 = $it.Prop1,
                //                                          Prop2 = $it.Prop2,
                //                                          ...
                //                                      }) 

                List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
                foreach (var prop in groupByTypeDef.Properties)
                {
                    wrapperTypeMemberAssignments.Add(Expression.Bind(GroupByWrapperType.GetMember(prop.Key).Single(), Expression.Property(source, prop.Key)));
                }

                
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(GroupByWrapperType), wrapperTypeMemberAssignments), source);
            }
            else
            {

                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(GroupByWrapperType), source);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, GroupByWrapperType);
        }
    }
}
