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
        private const string GroupByContainerProperty = "GroupByContainer";
        private ODataQuerySettings _settings;
        private IAssembliesResolver _assembliesResolver;
        private Type _elementType;
        private AggregationTransformationBase _transformation;

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
        }

        public IQueryable Bind(IQueryable query)
        {
            // TODO: After we switch to QueryNode result for parsing refactor to use 1 binder class per case
            ApplyAggregateClause aggregateClause = this._transformation as ApplyAggregateClause;
            bool grouped = false;
            IEnumerable<string> selectedStatements = null;
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

            // Answer is query.GroupBy($it => new GroupByWrapper() {...}).Select(GroupBy($it => new AggregationWrapper() {...}))
            // We are doing Grouping even if only aggregate was specified to have a IQuaryable after aggregation
            IQueryable grouping = BindGroupBy(query, selectedStatements);

            IQueryable result = BindSelect(grouping, aggregateClause, grouped);

            return result;
        }
        /// <summary>
        /// Generating Select clause 
        /// </summary>
        /// <param name="grouping"></param>
        /// <param name="aggregateClause"></param>
        /// <param name="grouped"></param>
        /// <returns></returns>
        private IQueryable BindSelect(IQueryable grouping, ApplyAggregateClause aggregateClause, bool grouped)
        {
            // Should return following expression
            // .Select($it => New AggregationWrapper() 
            //                  {
            //                      GroupByContainer = $it.Key.GroupByContainer,
            //                      Container = new { Alias = $it.AsQuaryable().Sum(i => i.AggregatableProperty)
            //                  })

            var groupingType = typeof(IGrouping<,>).MakeGenericType(GroupByWrapperType, this._elementType);
            ParameterExpression accum = Expression.Parameter(groupingType);
            Type wrapperType2 = typeof(AggregationWrapper<>).MakeGenericType(this._elementType);

            var keyProperty = ExpressionHelpers.GetPropertyAccessLambda(groupingType, "Key");

            List<MemberAssignment> wrapperTypeMemberAssignments2 = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (grouped)
            {

                var wrapperProperty2 = wrapperType2.GetProperty(GroupByContainerProperty);
                wrapperTypeMemberAssignments2.Add(Expression.Bind(wrapperProperty2, Expression.Property(Expression.Property(accum, "Key"), GroupByContainerProperty)));
            }

            // Setting Container property when we have aggregation clauses
            if (aggregateClause != null)
            {
                var wrapperProperty = wrapperType2.GetProperty("Container");
                var properties = new List<NamedPropertyExpression>();
                var propertyLambda = ExpressionHelpers.GetPropertyAccessLambda(this._elementType, aggregateClause.AggregatableProperty);
                MethodInfo aggregationMethod;

                switch (aggregateClause.AggregationMethod)
                {
                    case "min":
                        aggregationMethod = ExpressionHelperMethods.QueryableMin.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        break;
                    case "max":
                        aggregationMethod = ExpressionHelperMethods.QueryableMax.MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        break;
                    case "sum":
                        if (!ExpressionHelperMethods.QueryableSumGenerics.TryGetValue(propertyLambda.Body.Type, out aggregationMethod))
                        {
                            throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", aggregateClause.AggregationMethod, aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
                        }
                        aggregationMethod = aggregationMethod.MakeGenericMethod(this._elementType);
                        break;
                    case "average":
                        if (!ExpressionHelperMethods.QueryableAverageGenerics.TryGetValue(propertyLambda.Body.Type, out aggregationMethod))
                        {
                            throw new ODataException(Error.Format("Aggregation '{0}' not supported for property '{1}' of type '{2}'.", aggregateClause.AggregationMethod, aggregateClause.AggregatableProperty, propertyLambda.Body.Type));
                        }
                        aggregationMethod = aggregationMethod.MakeGenericMethod(this._elementType);
                        break;
                    default:
                        throw new ODataException(Error.Format("Aggregation method '{0}' is not supported.", aggregateClause.AggregationMethod));
                }

                // I substitute the element type for all generic arguments.                                                
                var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(this._elementType);

                Expression asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);
                Expression aggregationExpression = Expression.Call(null, aggregationMethod, asQuerableExpression, propertyLambda);

                properties.Add(new NamedPropertyExpression(Expression.Constant(aggregateClause.Alias), aggregationExpression));
                wrapperTypeMemberAssignments2.Add(Expression.Bind(wrapperProperty, PropertyContainer.CreatePropertyContainer(properties)));
            }

            var selectLambda = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType2), wrapperTypeMemberAssignments2), accum);

            var result = ExpressionHelpers.Select(grouping, selectLambda, groupingType);
            return result;
        }


        private Type GroupByWrapperType
        {
            get
            {
                // TODO: Add caching
                return typeof(GroupByWrapper<>).MakeGenericType(this._elementType);
            }
        }

        private IQueryable BindGroupBy(IQueryable query, IEnumerable<string> selectedStatements)
        {

            ParameterExpression source = Expression.Parameter(this._elementType);

            ConstructorInfo wrapperConstructor = GroupByWrapperType.GetConstructor(new Type[] { });
            NewExpression newExpression = Expression.New(wrapperConstructor);
            LambdaExpression groupLambda = null;
            if (selectedStatements != null && selectedStatements.Any())
            {
                // Generates expression
                // .GroupBy($it => new GroupingByClause()
                //                                      {
                //                                          GroupByContainer = new  { $it.Prop1, $it.Prop2, ...}
                //                                      }) 

                var properties = new List<NamedPropertyExpression>();
                foreach (var propName in selectedStatements)
                {
                    var propertyName = propName.Trim();
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), Expression.Property(source, propertyName)));
                }

                List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
                var wrapperProperty = GroupByWrapperType.GetProperty(GroupByContainerProperty);
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, PropertyContainer.CreatePropertyContainer(properties)));
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(GroupByWrapperType), wrapperTypeMemberAssignments), source);
            }
            else
            {

                // We do not have properties to aggregate
                // .GroupBy($it => new GroupingByClause())
                groupLambda = Expression.Lambda(Expression.New(GroupByWrapperType), source);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, GroupByWrapperType);
        }
    }
}
