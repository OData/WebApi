// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal class AggregationBinder : ExpressionBinderBase
    {
        private const string GroupByContainerProperty = "GroupByContainer";
        private Type _elementType;
        private TransformationNode _transformation;

        private ParameterExpression _lambdaParameter;

        private IEnumerable<AggregateExpressionBase> _aggregateExpressions;
        private IEnumerable<GroupByPropertyNode> _groupingProperties;

        private Type _groupByClrType;

        private bool _classicEF = false;

        internal AggregationBinder(ODataQuerySettings settings, IWebApiAssembliesResolver assembliesResolver, Type elementType,
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
                    _aggregateExpressions = FixCustomMethodReturnTypes(aggregateClause.AggregateExpressions);
                    ResultClrType = typeof(NoGroupByAggregationWrapper);
                    break;
                case TransformationNodeKind.GroupBy:
                    var groupByClause = this._transformation as GroupByTransformationNode;
                    _groupingProperties = groupByClause.GroupingProperties;
                    if (groupByClause.ChildTransformations != null)
                    {
                        if (groupByClause.ChildTransformations.Kind == TransformationNodeKind.Aggregate)
                        {
                            var aggregationNode = (AggregateTransformationNode)groupByClause.ChildTransformations;
                            _aggregateExpressions = FixCustomMethodReturnTypes(aggregationNode.AggregateExpressions);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    _groupByClrType = typeof(GroupByWrapper);
                    ResultClrType = typeof(AggregationWrapper);
                    break;
                default:
                    throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                        SRResources.NotSupportedTransformationKind, transformation.Kind));
            }

            _groupByClrType = _groupByClrType ?? typeof(NoGroupByWrapper);
        }

        private static Expression WrapDynamicCastIfNeeded(Expression propertyAccessor)
        {
            if (propertyAccessor.Type == typeof(object))
            {
                return Expression.Call(null, ExpressionHelperMethods.ConvertToDecimal, propertyAccessor);
            }

            return propertyAccessor;
        }

        private IEnumerable<AggregateExpressionBase> FixCustomMethodReturnTypes(IEnumerable<AggregateExpressionBase> aggregateExpressions)
        {
            return aggregateExpressions.Select(x =>
            {
                var ae = x as AggregateExpression;
                return ae != null ? FixCustomMethodReturnType(ae) : x;
            });
        }

        private AggregateExpression FixCustomMethodReturnType(AggregateExpression expression)
        {
            if (expression.Method != AggregationMethod.Custom)
            {
                return expression;
            }

            var customMethod = GetCustomMethod(expression);
            var typeReference = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(customMethod.ReturnType);
            return new AggregateExpression(expression.Expression, expression.MethodDefinition, expression.Alias, typeReference);
        }

        private MethodInfo GetCustomMethod(AggregateExpression expression)
        {
            var propertyLambda = Expression.Lambda(BindAccessor(expression.Expression), this._lambdaParameter);
            Type inputType = propertyLambda.Body.Type;

            string methodToken = expression.MethodDefinition.MethodLabel;
            var customFunctionAnnotations = Model.GetAnnotationValue<CustomAggregateMethodAnnotation>(Model);

            MethodInfo customMethod;
            if (!customFunctionAnnotations.GetMethodInfo(methodToken, inputType, out customMethod))
            {
                throw new ODataException(
                    Error.Format(
                        SRResources.AggregationNotSupportedForType,
                        expression.Method,
                        expression.Expression,
                        inputType));
            }

            return customMethod;
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
            Contract.Assert(query != null);

            this._classicEF = IsClassicEF(query);
            this.BaseQuery = query;
            EnsureFlattenedPropertyContainer(this._lambdaParameter);

            query = FlattenReferencedProperties(query);

            // Answer is query.GroupBy($it => new DynamicType1() {...}).Select($it => new DynamicType2() {...})
            // We are doing Grouping even if only aggregate was specified to have a IQuaryable after aggregation
            IQueryable grouping = BindGroupBy(query);

            IQueryable result = BindSelect(grouping);

            return result;
        }

        /// <summary>
        /// Pre flattens properties referenced in aggregate clause to avoid generation of nested queries by EF.
        /// For query like groupby((A), aggregate(B/C with max as Alias1, B/D with max as Alias2)) we need to generate 
        /// .Select(
        ///     $it => new FlattenninWrapper () {
        ///         Source = $it, // Will used in groupby stage
        ///         Container = new {
        ///             Value = $it.B.C
        ///             Next = new {
        ///                 Value = $it.B.D
        ///             }
        ///         }
        ///     }
        /// )
        /// Also we need to populate expressions to access B/C and B/D in aggregate stage. It will look like:
        /// B/C : $it.Container.Value
        /// B/D : $it.Container.Next.Value
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Query with Select that flattens properties</returns>
        private IQueryable FlattenReferencedProperties(IQueryable query)
        {
            if (_aggregateExpressions != null
                && _aggregateExpressions.OfType<AggregateExpression>().Any(e => e.Method != AggregationMethod.VirtualPropertyCount)
                && _groupingProperties != null
                && _groupingProperties.Any()
                && (FlattenedPropertyContainer == null || !FlattenedPropertyContainer.Any())
                )
            {
                var wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(this._elementType);
                var sourceProperty = wrapperType.GetProperty("Source");
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(sourceProperty, this._lambdaParameter));

                var properties = new List<NamedPropertyExpression>();
                var aliasIdx = 0;
                var aggParam = Expression.Parameter(wrapperType, "$it");
                var currentContainerExpression = Expression.Property(aggParam, GroupByContainerProperty);
                foreach (var aggExpression in _aggregateExpressions.OfType<AggregateExpression>().Where(e => e.Method != AggregationMethod.VirtualPropertyCount))
                {
                    var alias = "Property" + aliasIdx; // We just need unique alias, we aren't going to use it

                    // Add Value = $it.B.C
                    var propAccessExpression = BindAccessor(aggExpression.Expression);
                    var type = propAccessExpression.Type;
                    propAccessExpression = WrapConvert(propAccessExpression);
                    properties.Add(new NamedPropertyExpression(Expression.Constant(alias), propAccessExpression));

                    // Save $it.Container.Next.Value for future use
                    UnaryExpression flatAccessExpression = Expression.Convert(
                        Expression.Property(currentContainerExpression, "Value"),
                        type);
                    currentContainerExpression = Expression.Property(currentContainerExpression, "Next");
                    _preFlattenedMap.Add(aggExpression.Expression, flatAccessExpression);
                    aliasIdx++;
                }

                var wrapperProperty = ResultClrType.GetProperty(GroupByContainerProperty);

                wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

                var flatLambda = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), wta), _lambdaParameter);

                query = ExpressionHelpers.Select(query, flatLambda, this._elementType);

                // We applied flattening let .GroupBy know about it.
                this._lambdaParameter = aggParam;
                this._elementType = wrapperType;
            }

            return query;
        }

        private Dictionary<SingleValueNode, Expression> _preFlattenedMap = new Dictionary<SingleValueNode, Expression>();


        /// <summary>
        /// Checks IQueryable provider for need of EF6 oprimization
        /// </summary>
        /// <param name="query"></param>
        /// <returns>True if EF6 optimization are needed.</returns>
        internal virtual bool IsClassicEF(IQueryable query)
        {
            var providerNS = query.Provider.GetType().Namespace;
            return (providerNS == HandleNullPropagationOptionHelper.ObjectContextQueryProviderNamespaceEF6 
                || providerNS == HandleNullPropagationOptionHelper.EntityFrameworkQueryProviderNamespace);
        }

        private IQueryable BindSelect(IQueryable grouping)
        {
            // Should return following expression
            // .Select($it => New DynamicType2()
            //                  {
            //                      GroupByContainer = $it.Key.GroupByContainer // If groupby section present
            //                      Container => new AggregationPropertyContainer() {
            //                          Name = "Alias1",
            //                          Value = $it.AsQuaryable().Sum(i => i.AggregatableProperty),
            //                          Next = new LastInChain() {
            //                              Name = "Alias2",
            //                              Value = $it.AsQuaryable().Sum(i => i.AggregatableProperty)
            //                          }
            //                      }
            //                  })
            var groupingType = typeof(IGrouping<,>).MakeGenericType(this._groupByClrType, this._elementType);
            ParameterExpression accum = Expression.Parameter(groupingType, "$it");

            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            // Setting GroupByContainer property when previous step was grouping
            if (this._groupingProperties != null && this._groupingProperties.Any())
            {
                var wrapperProperty = this.ResultClrType.GetProperty(GroupByContainerProperty);

                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, Expression.Property(Expression.Property(accum, "Key"), GroupByContainerProperty)));
            }

            // Setting Container property when we have aggregation clauses
            if (_aggregateExpressions != null)
            {
                var properties = new List<NamedPropertyExpression>();
                foreach (var aggExpression in _aggregateExpressions)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(accum, aggExpression, this._elementType)));
                }

                var wrapperProperty = ResultClrType.GetProperty("Container");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));
            }

            var initilizedMember =
                Expression.MemberInit(Expression.New(ResultClrType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initilizedMember, accum);

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

        private Expression CreateAggregationExpression(ParameterExpression accum, AggregateExpressionBase expression, Type baseType)
        {
            switch (expression.AggregateKind)
            {
                case AggregateExpressionKind.PropertyAggregate:
                    return CreatePropertyAggregateExpression(accum, expression as AggregateExpression, baseType);
                case AggregateExpressionKind.EntitySetAggregate:
                    return CreateEntitySetAggregateExpression(accum, expression as EntitySetAggregateExpression, baseType);
                default:
                    throw new ODataException(Error.Format(SRResources.AggregateKindNotSupported, expression.AggregateKind));
            }
        }

        private Expression CreateEntitySetAggregateExpression(
            ParameterExpression accum, EntitySetAggregateExpression expression, Type baseType)
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

            // Nested properties
            // Create dynamicTypeWrapper to encapsulate the aggregate result
            var properties = new List<NamedPropertyExpression>();
            foreach (var aggExpression in expression.Children)
            {
                properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(innerAccum, aggExpression, selectedElementType)));
            }

            var nestedResultType = typeof(EntitySetAggregationWrapper);
            var wrapperProperty = nestedResultType.GetProperty("Container");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            var initializedMember =
                Expression.MemberInit(Expression.New(nestedResultType), wrapperTypeMemberAssignments);
            var selectLambda = Expression.Lambda(initializedMember, innerAccum);

            // Get select method
            MethodInfo selectMethod =
                ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(
                    groupingType,
                    selectLambda.Body.Type);

            return Expression.Call(null, selectMethod, groupedEntitySet, selectLambda);
        }

        private Expression CreatePropertyAggregateExpression(ParameterExpression accum, AggregateExpression expression, Type baseType)
        {
            // accum type is IGrouping<,baseType> that implements IEnumerable<baseType> 
            // we need cast it to IEnumerable<baseType> during expression building (IEnumerable)$it
            // however for EF6 we need to use $it.AsQueryable() due to limitations in types of casts that will properly translated
            Expression asQuerableExpression = null;
            if (_classicEF)
            {
                var asQuerableMethod = ExpressionHelperMethods.QueryableAsQueryable.MakeGenericMethod(baseType);
                asQuerableExpression = Expression.Call(null, asQuerableMethod, accum);
            }
            else
            {
                var queryableType = typeof(IEnumerable<>).MakeGenericType(baseType);
                asQuerableExpression = Expression.Convert(accum, queryableType);
            }

            // $count is a virtual property, so there's not a propertyLambda to create.
            if (expression.Method == AggregationMethod.VirtualPropertyCount)
            {
                var countMethod = (_classicEF 
                    ? ExpressionHelperMethods.QueryableCountGeneric
                    : ExpressionHelperMethods.EnumerableCountGeneric).MakeGenericMethod(baseType);
                return WrapConvert(Expression.Call(null, countMethod, asQuerableExpression));
            }

            Expression body;

            var lambdaParameter = baseType == this._elementType ? this._lambdaParameter : Expression.Parameter(baseType, "$it");
            if (!this._preFlattenedMap.TryGetValue(expression.Expression, out body))
            {
                body = BindAccessor(expression.Expression);
            }
            LambdaExpression propertyLambda = Expression.Lambda(body, lambdaParameter);

            Expression aggregationExpression;

            switch (expression.Method)
            {
                case AggregationMethod.Min:
                    {
                        var minMethod = (_classicEF
                            ? ExpressionHelperMethods.QueryableMin
                            : ExpressionHelperMethods.EnumerableMin).MakeGenericMethod(baseType,
                            propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Max:
                    {
                        var maxMethod = (_classicEF
                            ? ExpressionHelperMethods.QueryableMax
                            : ExpressionHelperMethods.EnumerableMax).MakeGenericMethod(baseType,
                            propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, maxMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Sum:
                    {
                        MethodInfo sumGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (
                            !(_classicEF 
                                ? ExpressionHelperMethods.QueryableSumGenerics
                                : ExpressionHelperMethods.EnumerableSumGenerics).TryGetValue(propertyExpression.Type,
                                out sumGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var sumMethod = sumGenericMethod.MakeGenericMethod(baseType);
                        aggregationExpression = Expression.Call(null, sumMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.Average:
                    {
                        MethodInfo averageGenericMethod;
                        // For Dynamic properties cast to decimal
                        Expression propertyExpression = WrapDynamicCastIfNeeded(body);
                        propertyLambda = Expression.Lambda(propertyExpression, lambdaParameter);

                        if (
                            !(_classicEF
                                ? ExpressionHelperMethods.QueryableAverageGenerics
                                : ExpressionHelperMethods.EnumerableAverageGenerics).TryGetValue(propertyExpression.Type,
                                out averageGenericMethod))
                        {
                            throw new ODataException(Error.Format(SRResources.AggregationNotSupportedForType,
                                expression.Method, expression.Expression, propertyExpression.Type));
                        }

                        var averageMethod = averageGenericMethod.MakeGenericMethod(baseType);
                        aggregationExpression = Expression.Call(null, averageMethod, asQuerableExpression, propertyLambda);

                        // For Dynamic properties cast back to object
                        if (propertyLambda.Type == typeof(object))
                        {
                            aggregationExpression = Expression.Convert(aggregationExpression, typeof(object));
                        }
                    }
                    break;
                case AggregationMethod.CountDistinct:
                    {
                        // I select the specific field
                        var selectMethod =
                            (_classicEF
                                ? ExpressionHelperMethods.QueryableSelectGeneric
                                : ExpressionHelperMethods.EnumerableSelectGeneric).MakeGenericMethod(this._elementType,
                                propertyLambda.Body.Type);
                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQuerableExpression,
                            propertyLambda);

                        // I run distinct over the set of items
                        var distinctMethod =
                            (_classicEF 
                                ? ExpressionHelperMethods.QueryableDistinct
                                : ExpressionHelperMethods.EnumerableDistinct).MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                        // I count the distinct items as the aggregation expression
                        var countMethod =
                            (_classicEF
                                ? ExpressionHelperMethods.QueryableCountGeneric
                                : ExpressionHelperMethods.EnumerableCountGeneric).MakeGenericMethod(propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, countMethod, distinctExpression);
                    }
                    break;
                case AggregationMethod.Custom:
                    {
                        MethodInfo customMethod = GetCustomMethod(expression);
                        var selectMethod =
                            (_classicEF
                                ? ExpressionHelperMethods.QueryableSelectGeneric
                                : ExpressionHelperMethods.EnumerableSelectGeneric).MakeGenericMethod(this._elementType, propertyLambda.Body.Type);
                        var selectExpression = Expression.Call(null, selectMethod, asQuerableExpression, propertyLambda);
                        aggregationExpression = Expression.Call(null, customMethod, selectExpression);
                    }
                    break;
                default:
                    throw new ODataException(Error.Format(SRResources.AggregationMethodNotSupported, expression.Method));
            }

            return WrapConvert(aggregationExpression);
        }

        private Expression WrapConvert(Expression expression)
        {
            // Expression that we are generating looks like Value = $it.PropertyName where Value is defined as object and PropertyName can be value 
            // Proper .NET expression must look like as Value = (object) $it.PropertyName for proper boxing or AccessViolationException will be thrown
            // Cast to object isn't translatable by EF6 as a result skipping (object) in that case
            return (this._classicEF || !expression.Type.IsValueType)
                ? expression
                : Expression.Convert(expression, typeof(object));
        }

        private Expression BindAccessor(QueryNode node, Expression baseElement = null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.ResourceRangeVariableReference:
                    return this._lambdaParameter.Type.IsGenericType && this._lambdaParameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                            ? (Expression)Expression.Property(this._lambdaParameter, "Source")
                            : this._lambdaParameter;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propAccessNode = node as SingleValuePropertyAccessNode;
                    return CreatePropertyAccessExpression(BindAccessor(propAccessNode.Source, baseElement), propAccessNode.Property, GetFullPropertyPath(propAccessNode));
                case QueryNodeKind.AggregatedCollectionPropertyNode:
                    var aggPropAccessNode = node as AggregatedCollectionPropertyNode;
                    return CreatePropertyAccessExpression(BindAccessor(aggPropAccessNode.Source, baseElement), aggPropAccessNode.Property);
                case QueryNodeKind.SingleComplexNode:
                    var singleComplexNode = node as SingleComplexNode;
                    return CreatePropertyAccessExpression(BindAccessor(singleComplexNode.Source, baseElement), singleComplexNode.Property, GetFullPropertyPath(singleComplexNode));
                case QueryNodeKind.SingleValueOpenPropertyAccess:
                    var openNode = node as SingleValueOpenPropertyAccessNode;
                    return GetFlattenedPropertyExpression(openNode.Name) ?? CreateOpenPropertyAccessExpression(openNode);
                case QueryNodeKind.None:
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = (SingleNavigationNode)node;
                    return CreatePropertyAccessExpression(BindAccessor(navNode.Source), navNode.NavigationProperty);
                case QueryNodeKind.BinaryOperator:
                    var binaryNode = (BinaryOperatorNode)node;
                    var leftExpression = BindAccessor(binaryNode.Left, baseElement);
                    var rightExpression = BindAccessor(binaryNode.Right, baseElement);
                    return CreateBinaryExpression(binaryNode.OperatorKind, leftExpression, rightExpression,
                        liftToNull: true);
                case QueryNodeKind.Convert:
                    var convertNode = (ConvertNode)node;
                    return CreateConvertExpression(convertNode, BindAccessor(convertNode.Source, baseElement));
                case QueryNodeKind.CollectionNavigationNode:
                    return baseElement ?? this._lambdaParameter;
                default:
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, node.Kind,
                        typeof(AggregationBinder).Name);
            }
        }

        private Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property, string propertyPath = null)
        {
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, Model);
            propertyPath = propertyPath ?? propertyName;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && IsNullable(source.Type) &&
                source != this._lambdaParameter)
            {
                Expression cleanSource = RemoveInnerNullPropagation(source);
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

        private Expression CreateOpenPropertyAccessExpression(SingleValueOpenPropertyAccessNode openNode)
        {
            Expression sourceAccessor = BindAccessor(openNode.Source);

            // First check that property exists in source
            // It's the case when we are apply transformation based on earlier transformation
            if (sourceAccessor.Type.GetProperty(openNode.Name) != null)
            {
                return Expression.Property(sourceAccessor, openNode.Name);
            }

            // Property doesn't exists go for dynamic properties dictionary
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);
            MemberExpression propertyAccessExpression = Expression.Property(sourceAccessor, prop.Name);
            IndexExpression readDictionaryIndexerExpression = Expression.Property(propertyAccessExpression,
                            DictionaryStringObjectIndexerName, Expression.Constant(openNode.Name));
            MethodCallExpression containsKeyExpression = Expression.Call(propertyAccessExpression,
                propertyAccessExpression.Type.GetMethod("ContainsKey"), Expression.Constant(openNode.Name));
            ConstantExpression nullExpression = Expression.Constant(null);

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

        private IQueryable BindGroupBy(IQueryable query)
        {
            LambdaExpression groupLambda = null;
            Type elementType = query.ElementType;
            if (_groupingProperties != null && _groupingProperties.Any())
            {
                // Generates expression
                // .GroupBy($it => new DynamicTypeWrapper()
                //                                      {
                //                                           GroupByContainer => new AggregationPropertyContainer() {
                //                                               Name = "Prop1",
                //                                               Value = $it.Prop1,
                //                                               Next = new AggregationPropertyContainer() {
                //                                                   Name = "Prop2",
                //                                                   Value = $it.Prop2, // int
                //                                                   Next = new LastInChain() {
                //                                                       Name = "Prop3",
                //                                                       Value = $it.Prop3
                //                                                   }
                //                                               }
                //                                           }
                //                                      })
                List<NamedPropertyExpression> properties = CreateGroupByMemberAssignments(_groupingProperties);

                var wrapperProperty = typeof(GroupByWrapper).GetProperty(GroupByContainerProperty);
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wta), _lambdaParameter);
            }
            else
            {
                // We do not have properties to aggregate
                // .GroupBy($it => new NoGroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(this._groupByClrType), this._lambdaParameter);
            }

            return ExpressionHelpers.GroupBy(query, groupLambda, elementType, this._groupByClrType);
        }

        private List<NamedPropertyExpression> CreateGroupByMemberAssignments(IEnumerable<GroupByPropertyNode> nodes)
        {
            var properties = new List<NamedPropertyExpression>();
            foreach (var grpProp in nodes)
            {
                var propertyName = grpProp.Name;
                if (grpProp.Expression != null)
                {
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), WrapConvert(BindAccessor(grpProp.Expression))));
                }
                else
                {
                    var wrapperProperty = typeof(GroupByWrapper).GetProperty(GroupByContainerProperty);
                    List<MemberAssignment> wta = new List<MemberAssignment>();
                    wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(CreateGroupByMemberAssignments(grpProp.ChildTransformations))));
                    properties.Add(new NamedPropertyExpression(Expression.Constant(propertyName), Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wta)));
                }
            }

            return properties;
        }
    }
}
