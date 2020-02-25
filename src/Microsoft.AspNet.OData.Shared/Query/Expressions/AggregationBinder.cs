// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
    internal class AggregationBinder : TransformationBinderBase
    {
        private const string GroupByContainerProperty = "GroupByContainer";
        private TransformationNode _transformation;

        private IEnumerable<AggregateExpressionBase> _aggregateExpressions;
        private IEnumerable<GroupByPropertyNode> _groupingProperties;

        private Type _groupByClrType;

        internal AggregationBinder(ODataQuerySettings settings, IWebApiAssembliesResolver assembliesResolver, Type elementType,
            IEdmModel model, TransformationNode transformation)
            : base(settings, assembliesResolver, elementType, model)
        {
            Contract.Assert(transformation != null);

            _transformation = transformation;

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
            var propertyLambda = Expression.Lambda(BindAccessor(expression.Expression), this.LambdaParameter);
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

        public IQueryable Bind(IQueryable query)
        {
            PreprocessQuery(query);

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
                && (FlattenedPropertyContainer == null || !FlattenedPropertyContainer.Any()))
            {
                var wrapperType = typeof(FlatteningWrapper<>).MakeGenericType(this.ElementType);
                var sourceProperty = wrapperType.GetProperty("Source");
                List<MemberAssignment> wta = new List<MemberAssignment>();
                wta.Add(Expression.Bind(sourceProperty, this.LambdaParameter));

                var aggrregatedPropertiesToFlatten = _aggregateExpressions.OfType<AggregateExpression>().Where(e => e.Method != AggregationMethod.VirtualPropertyCount).ToList();
                // Generated Select will be stack like, meaning that first property in the list will be deepest one
                // For example if we add $it.B.C, $it.B.D, select will look like
                // new {
                //      Value = $it.B.C
                //      Next = new {
                //          Value = $it.B.D
                //      }
                // }
                // We are generated references (in currentContainerExpression) from  the begining of the  Select ($it.Value, then $it.Next.Value etc.)
                // We have proper match we need insert properties in reverse order
                // After this 
                // properties = { $it.B.D, $it.B.C}
                // _preFlattendMAp = { {$it.B.C, $it.Value}, {$it.B.D, $it.Next.Value} }
                var properties = new NamedPropertyExpression[aggrregatedPropertiesToFlatten.Count];
                var aliasIdx = aggrregatedPropertiesToFlatten.Count - 1;
                var aggParam = Expression.Parameter(wrapperType, "$it");
                var currentContainerExpression = Expression.Property(aggParam, GroupByContainerProperty);
                foreach (var aggExpression in aggrregatedPropertiesToFlatten)
                {
                    var alias = "Property" + aliasIdx.ToString(CultureInfo.CurrentCulture); // We just need unique alias, we aren't going to use it

                    // Add Value = $it.B.C
                    var propAccessExpression = BindAccessor(aggExpression.Expression);
                    var type = propAccessExpression.Type;
                    propAccessExpression = WrapConvert(propAccessExpression);
                    properties[aliasIdx] = new NamedPropertyExpression(Expression.Constant(alias), propAccessExpression);

                    // Save $it.Container.Next.Value for future use
                    UnaryExpression flatAccessExpression = Expression.Convert(
                        Expression.Property(currentContainerExpression, "Value"),
                        type);
                    currentContainerExpression = Expression.Property(currentContainerExpression, "Next");
                    _preFlattenedMap.Add(aggExpression.Expression, flatAccessExpression);
                    aliasIdx--;
                }

                var wrapperProperty = ResultClrType.GetProperty(GroupByContainerProperty);

                wta.Add(Expression.Bind(wrapperProperty, AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

                var flatLambda = Expression.Lambda(Expression.MemberInit(Expression.New(wrapperType), wta), LambdaParameter);

                query = ExpressionHelpers.Select(query, flatLambda, this.ElementType);

                // We applied flattening let .GroupBy know about it.
                this.SetElementType(wrapperType, aggParam);
            }

            return query;
        }

        private Dictionary<SingleValueNode, Expression> _preFlattenedMap = new Dictionary<SingleValueNode, Expression>();

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
            var groupingType = typeof(IGrouping<,>).MakeGenericType(this._groupByClrType, this.ElementType);
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
                    properties.Add(new NamedPropertyExpression(Expression.Constant(aggExpression.Alias), CreateAggregationExpression(accum, aggExpression, this.ElementType)));
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
            if (ClassicEF)
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
                var countMethod = (ClassicEF 
                    ? ExpressionHelperMethods.QueryableCountGeneric
                    : ExpressionHelperMethods.EnumerableCountGeneric).MakeGenericMethod(baseType);
                return WrapConvert(Expression.Call(null, countMethod, asQuerableExpression));
            }

            Expression body;

            var lambdaParameter = baseType == this.ElementType ? this.LambdaParameter : Expression.Parameter(baseType, "$it");
            if (!this._preFlattenedMap.TryGetValue(expression.Expression, out body))
            {
                body = BindAccessor(expression.Expression, lambdaParameter);
            }
            LambdaExpression propertyLambda = Expression.Lambda(body, lambdaParameter);

            Expression aggregationExpression;

            switch (expression.Method)
            {
                case AggregationMethod.Min:
                    {
                        var minMethod = (ClassicEF
                            ? ExpressionHelperMethods.QueryableMin
                            : ExpressionHelperMethods.EnumerableMin).MakeGenericMethod(baseType,
                            propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, minMethod, asQuerableExpression, propertyLambda);
                    }
                    break;
                case AggregationMethod.Max:
                    {
                        var maxMethod = (ClassicEF
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
                            !(ClassicEF 
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
                            !(ClassicEF
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
                            (ClassicEF
                                ? ExpressionHelperMethods.QueryableSelectGeneric
                                : ExpressionHelperMethods.EnumerableSelectGeneric).MakeGenericMethod(this.ElementType,
                                propertyLambda.Body.Type);
                        Expression queryableSelectExpression = Expression.Call(null, selectMethod, asQuerableExpression,
                            propertyLambda);

                        // I run distinct over the set of items
                        var distinctMethod =
                            (ClassicEF 
                                ? ExpressionHelperMethods.QueryableDistinct
                                : ExpressionHelperMethods.EnumerableDistinct).MakeGenericMethod(propertyLambda.Body.Type);
                        Expression distinctExpression = Expression.Call(null, distinctMethod, queryableSelectExpression);

                        // I count the distinct items as the aggregation expression
                        var countMethod =
                            (ClassicEF
                                ? ExpressionHelperMethods.QueryableCountGeneric
                                : ExpressionHelperMethods.EnumerableCountGeneric).MakeGenericMethod(propertyLambda.Body.Type);
                        aggregationExpression = Expression.Call(null, countMethod, distinctExpression);
                    }
                    break;
                case AggregationMethod.Custom:
                    {
                        MethodInfo customMethod = GetCustomMethod(expression);
                        var selectMethod =
                            (ClassicEF
                                ? ExpressionHelperMethods.QueryableSelectGeneric
                                : ExpressionHelperMethods.EnumerableSelectGeneric).MakeGenericMethod(this.ElementType, propertyLambda.Body.Type);
                        var selectExpression = Expression.Call(null, selectMethod, asQuerableExpression, propertyLambda);
                        aggregationExpression = Expression.Call(null, customMethod, selectExpression);
                    }
                    break;
                default:
                    throw new ODataException(Error.Format(SRResources.AggregationMethodNotSupported, expression.Method));
            }

            return WrapConvert(aggregationExpression);
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
                groupLambda = Expression.Lambda(Expression.MemberInit(Expression.New(typeof(GroupByWrapper)), wta), LambdaParameter);
            }
            else
            {
                // We do not have properties to aggregate
                // .GroupBy($it => new NoGroupByWrapper())
                groupLambda = Expression.Lambda(Expression.New(this._groupByClrType), this.LambdaParameter);
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
