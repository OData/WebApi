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
    internal class AggregationBinder
    {
        private ODataQuerySettings _settings;
        private IEdmModel _model;
        private IAssembliesResolver _assembliesResolver;
        private Type _elementType;
        private AggregationTransformationBase _transformation;

        ParameterExpression _source; 

        private ApplyAggregateClause _aggregateClause;
        //private IEnumerable<string> _selectedStatements;
        private IList<ExpressionClause> _selectedProperties;
        private bool _grouped = false;
        private Type _groupByWrapperType;
        private TypeDefinition _groupByTypeDef;


        public AggregationBinder(ODataQuerySettings settings, IAssembliesResolver assembliesResolver, Type elementType, IEdmModel model, AggregationTransformationBase transformation)
        {
            Contract.Assert(settings != null);
            Contract.Assert(model != null);
            Contract.Assert(assembliesResolver != null);
            Contract.Assert(elementType != null);
            Contract.Assert(transformation != null);


            _settings = settings;
            _model = model;
            _assembliesResolver = assembliesResolver;
            _elementType = elementType;
            _transformation = transformation;

            // TODO: Do we need to use Range?
            this._source = Expression.Parameter(this._elementType);

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
                    //_selectedStatements = groupByClause.SelectedStatements;
                    _selectedProperties = groupByClause.SelectedPropertiesExpressions;
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
            ParameterExpression source = Expression.Parameter(this._elementType);
            if (_selectedProperties != null && _selectedProperties.Any())
            {
                foreach (var prop in _selectedProperties)
                {
                    var property = ((SingleValuePropertyAccessNode)prop.Expression).Property;
                    _groupByTypeDef.Properties.Add(property.Name, EdmLibHelpers.GetClrType(property.Type, _model));
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

            var keyProperty = ExpressionHelpers.GetPropertyAccessLambda(groupingType, "Key");

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
                LambdaExpression propertyLambda = ExpressionHelpers.GetPropertyAccessLambda(this._elementType, _aggregateClause.AggregatableProperty);
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


        private Expression BindProperty(SingleValueNode node)
        {
            switch(node.Kind)
            {
                case QueryNodeKind.EntityRangeVariableReference:
                    return this._source;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = (SingleValuePropertyAccessNode)node;
                    return Expression.Property(BindProperty(propAccessNode.Source), propAccessNode.Property.Name);
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = (SingleNavigationNode)node;
                    return Expression.Property(BindProperty(navNode.Source), navNode.NavigationProperty.Name);

                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind, typeof(AggregationBinder).Name);
            }
        }

        private IQueryable BindGroupBy(IQueryable query)
        {

            

            ConstructorInfo wrapperConstructor = _groupByWrapperType.GetConstructor(new Type[] { });
            NewExpression newExpression = Expression.New(wrapperConstructor);
            LambdaExpression groupLambda = null;
            if (_selectedProperties != null && _selectedProperties.Any())
            {
                // Generates expression
                // .GroupBy($it => new DynamicType1()
                //                                      {
                //                                          Prop1 = $it.Prop1,
                //                                          Prop2 = $it.Prop2,
                //                                          ...
                //                                      }) 

                List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();
                foreach (var prop in _selectedProperties)
                {
                    var exp = ((SingleValuePropertyAccessNode)prop.Expression);
                    wrapperTypeMemberAssignments.Add(Expression.Bind(_groupByWrapperType.GetMember(exp.Property.Name).Single(), BindProperty(prop.Expression)));
                    //_groupByTypeDef.Properties.Add(property.Name, EdmLibHelpers.GetClrType(property.Type, _model));
                }
                //foreach (var prop in _groupByTypeDef.Properties)
                //{
                //    wrapperTypeMemberAssignments.Add(Expression.Bind(_groupByWrapperType.GetMember(prop.Key).Single(), Expression.Property(source, prop.Key)));
                //}


                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(_groupByWrapperType), wrapperTypeMemberAssignments), this._source);
            }
            else
            {

                // We do not have properties to aggregate
                // .GroupBy($it => new GroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(_groupByWrapperType), this._source);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, this._elementType, _groupByWrapperType);
        }
    }
}
