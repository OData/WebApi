﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Applies the given <see cref="SelectExpandQueryOption"/> to the given <see cref="IQueryable"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "Class coupling acceptable.")]
    public class SelectExpandBinder
    {
        private SelectExpandQueryOption _selectExpandQuery;
        private ODataQueryContext _context;
        private IEdmModel _model;
        private ODataQuerySettings _settings;
        private string _modelID;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectExpandBinder"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> to use during binding.</param>
        /// <param name="selectExpandQuery">The <see cref="SelectExpandQueryOption"/> that contains the OData $select and $expand query options.</param>
        protected internal SelectExpandBinder(ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(settings != null);
            Contract.Assert(selectExpandQuery != null);
            Contract.Assert(selectExpandQuery.Context != null);
            Contract.Assert(selectExpandQuery.Context.Model != null);
            Contract.Assert(settings.HandleNullPropagation != HandleNullPropagationOption.Default);

            _selectExpandQuery = selectExpandQuery;
            _context = selectExpandQuery.Context;
            _model = _context.Model;
            _modelID = ModelContainer.GetModelID(_model);
            _settings = settings;
        }
        
        /// <summary>
        /// Applies the $select and $expand query options to the given entity.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        protected internal virtual object Bind(object entity)
        {
            Contract.Assert(entity != null);

            LambdaExpression projectionLambda = GetProjectionLambda();

            // TODO: cache this ?
            return projectionLambda.Compile().DynamicInvoke(entity);
        }

        /// <summary>
        /// Applies the $select and $expand query options to the given <see cref="IQueryable"/>.
        /// </summary>
        /// <param name="queryable">The original <see cref="IQueryable"/>.</param>
        protected internal virtual IQueryable Bind(IQueryable queryable)
        {
            Type elementType = _selectExpandQuery.Context.ElementClrType;

            LambdaExpression projectionLambda = GetProjectionLambda();

            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(elementType, projectionLambda.Body.Type);
            return selectMethod.Invoke(null, new object[] { queryable, projectionLambda }) as IQueryable;
        }

        private LambdaExpression GetProjectionLambda()
        {
            Type elementType = _selectExpandQuery.Context.ElementClrType;
            IEdmNavigationSource navigationSource = _selectExpandQuery.Context.NavigationSource;
            ParameterExpression source = Expression.Parameter(elementType);

            // expression looks like -> new Wrapper { Instance = source , Properties = "...", Container = new PropertyContainer { ... } }
            Expression projectionExpression = ProjectElement(source, _selectExpandQuery.SelectExpandClause, _context.ElementType as IEdmStructuredType, navigationSource);

            // expression looks like -> source => new Wrapper { Instance = source .... }
            LambdaExpression projectionLambdaExpression = Expression.Lambda(projectionExpression, source);

            return projectionLambdaExpression;
        }

        internal Expression ProjectAsWrapper(Expression source, SelectExpandClause selectExpandClause,
            IEdmEntityType entityType, IEdmNavigationSource navigationSource, ExpandedReferenceSelectItem expandedItem = null,
            int? modelBoundPageSize = null)
        {
            Type elementType;
            if (TypeHelper.IsCollection(source.Type, out elementType))
            {
                // new CollectionWrapper<ElementType> { Instance = source.Select(s => new Wrapper { ... }) };
                return ProjectCollection(source, elementType, selectExpandClause, entityType, navigationSource, expandedItem,
                    modelBoundPageSize);
            }
            else
            {
                // new Wrapper { v1 = source.property ... }
                return ProjectElement(source, selectExpandClause, entityType, navigationSource);
            }
        }

        /// <summary>
        /// Returns an <see cref="Expression"/> that represents the name of <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="elementType">The EDM entity type of the provided <paramref name="source"/>.</param>
        /// <param name="edmProperty">The EDM property which name expression to return.</param>
        /// <param name="source">The source that contains the <paramref name="edmProperty"/>.</param>
        /// <returns>The property name <see cref="Expression"/>.</returns>
        protected internal virtual Expression CreatePropertyNameExpression(IEdmStructuredType elementType, IEdmProperty edmProperty, Expression source)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(edmProperty != null);
            Contract.Assert(source != null);

            IEdmStructuredType declaringType = edmProperty.DeclaringType as IEdmStructuredType;

            Contract.Assert(declaringType != null, "Unstructured types cannot be projected.");

            // derived navigation property using cast
            if (elementType != declaringType)
            {
                Type originalType = EdmLibHelpers.GetClrType(elementType, _model);
                Type castType = EdmLibHelpers.GetClrType(declaringType, _model);
                if (castType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, declaringType.FullTypeName()));
                }

                if (!castType.IsAssignableFrom(originalType))
                {
                    // Expression
                    //          source is navigationPropertyDeclaringType ? propertyName : null
                    return Expression.Condition(
                        test: Expression.TypeIs(source, castType),
                        ifTrue: Expression.Constant(edmProperty.Name),
                        ifFalse: Expression.Constant(null, typeof(string)));
                }
            }

            // Expression
            //          "propertyName"
            return Expression.Constant(edmProperty.Name);
        }

        internal Expression CreatePropertyValueExpression(IEdmStructuredType elementType, IEdmProperty property, Expression source)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(property != null);
            Contract.Assert(source != null);

            return CreatePropertyValueExpressionWithFilter(elementType, property, source, null);
        }

        /// <summary>
        /// Returns an <see cref="Expression"/> that represents the value of <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="elementType">The EDM entity type of the provided <paramref name="source"/>.</param>
        /// <param name="edmProperty">The EDM property which value expression to return.</param>
        /// <param name="source">The source that contains the <paramref name="edmProperty"/>.</param>
        /// <param name="expandItem">The <see cref="ExpandedReferenceSelectItem"/> that describes how to expand <paramref name="edmProperty"/>.</param>
        /// <returns>The property value <see cref="Expression"/>.</returns>
        protected internal virtual Expression CreatePropertyValueExpressionWithFilter(IEdmStructuredType elementType, IEdmProperty edmProperty,
            Expression source, ExpandedReferenceSelectItem expandItem)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(edmProperty != null);
            Contract.Assert(source != null);

            FilterClause filterClause = expandItem != null ? expandItem.FilterOption : null;

            IEdmStructuredType declaringType;
            IEdmStructuredType currentType = elementType;
            if (expandItem != null)
            {
                foreach (ODataPathSegment segment in expandItem.PathToNavigationProperty)
                {
                    string currentProperty = String.Empty;
                    PropertySegment propertyAccessPathSegment =
                         segment as PropertySegment;
                    if (propertyAccessPathSegment != null)
                    {
                        IEdmProperty propertyInPath = propertyAccessPathSegment.Property;
                        currentProperty = EdmLibHelpers.GetClrPropertyName(propertyInPath, _model);
                        declaringType = propertyInPath.DeclaringType;
                        Contract.Assert(!String.IsNullOrEmpty(currentProperty), "Property name could not be found on the model.");
                        if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
                        {
                            // create expression similar to: 'source == null ? null : propertyValue'
                            if (declaringType != currentType)
                            {
                                Type castType = EdmLibHelpers.GetClrType(edmProperty.DeclaringType, _model);
                                if (castType == null)
                                {
                                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType,
                                        edmProperty.DeclaringType.FullTypeName()));
                                }

                                source = Expression.TypeAs(source, castType);
                            }
                            Expression propertyExpression = CreatePropertyAccessExpression(source, currentProperty);
                            Type nullablePropType = TypeHelper.ToNullable(propertyExpression.Type);

                            source = Expression.Condition(
                                test: Expression.Equal(propertyExpression, Expression.Constant(value: null)),
                                ifTrue: Expression.Constant(value: null, type: nullablePropType),
                                ifFalse: propertyExpression);
                        }
                        else
                        {
                            source = CreatePropertyAccessExpression(source, currentProperty);
                        }

                        currentType = propertyInPath.Type.ToStructuredType();
                    }
                    else
                    {
                        TypeSegment typeSegment = segment as TypeSegment;
                        if (typeSegment != null)
                        {
                            Type castType = EdmLibHelpers.GetClrType(typeSegment.EdmType, _model);
                            source = Expression.TypeAs(source, castType);
                            currentType = typeSegment.EdmType as IEdmStructuredType;
                        }
                    }
                }
            }

            // derived property using cast
            if (currentType != edmProperty.DeclaringType)
            {
                Type castType = EdmLibHelpers.GetClrType(edmProperty.DeclaringType, _model);
                if (castType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType,
                        edmProperty.DeclaringType.FullTypeName()));
                }

                source = Expression.TypeAs(source, castType);
            }

            Expression propertyValue = CreatePropertyAccessExpression(source, edmProperty);
            Type nullablePropertyType = TypeHelper.ToNullable(propertyValue.Type);
            Expression nullablePropertyValue = ExpressionHelpers.ToNullable(propertyValue);

            if (filterClause != null)
            {
                bool isCollection = edmProperty.Type.IsCollection();

                IEdmTypeReference edmElementType = (isCollection ? edmProperty.Type.AsCollection().ElementType() : edmProperty.Type);
                Type clrElementType = EdmLibHelpers.GetClrType(edmElementType, _model);
                if (clrElementType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType,
                        edmElementType.FullName()));
                }

                Expression filterResult = nullablePropertyValue;

                ODataQuerySettings querySettings = new ODataQuerySettings()
                {
                    HandleNullPropagation = HandleNullPropagationOption.True,
                };

                if (isCollection)
                {
                    Expression filterSource = nullablePropertyValue;

                    // TODO: Implement proper support for $select/$expand after $apply
                    Expression filterPredicate = FilterBinder.Bind(null, filterClause, clrElementType, _context, querySettings);
                    filterResult = Expression.Call(
                        ExpressionHelperMethods.EnumerableWhereGeneric.MakeGenericMethod(clrElementType),
                        filterSource,
                        filterPredicate);

                    nullablePropertyType = filterResult.Type;
                }
                else if (_settings.HandleReferenceNavigationPropertyExpandFilter)
                {
                    LambdaExpression filterLambdaExpression = FilterBinder.Bind(null, filterClause, clrElementType, _context, querySettings) as LambdaExpression;
                    if (filterLambdaExpression == null)
                    {
                        throw new ODataException(Error.Format(SRResources.ExpandFilterExpressionNotLambdaExpression,
                            edmProperty.Name, "LambdaExpression"));
                    }

                    ParameterExpression filterParameter = filterLambdaExpression.Parameters.First();
                    Expression predicateExpression = new ReferenceNavigationPropertyExpandFilterVisitor(filterParameter, nullablePropertyValue).Visit(filterLambdaExpression.Body);

                    // create expression similar to: 'predicateExpression == true ? nullablePropertyValue : null'
                    filterResult = Expression.Condition(
                        test: predicateExpression,
                        ifTrue: nullablePropertyValue,
                        ifFalse: Expression.Constant(value: null, type: nullablePropertyType));
                }

                if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // create expression similar to: 'nullablePropertyValue == null ? null : filterResult'
                    nullablePropertyValue = Expression.Condition(
                        test: Expression.Equal(nullablePropertyValue, Expression.Constant(value: null)),
                        ifTrue: Expression.Constant(value: null, type: nullablePropertyType),
                        ifFalse: filterResult);
                }
                else
                {
                    nullablePropertyValue = filterResult;
                }
            }

            if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // create expression similar to: 'source == null ? null : propertyValue'
                propertyValue = Expression.Condition(
                    test: Expression.Equal(source, Expression.Constant(value: null)),
                    ifTrue: Expression.Constant(value: null, type: nullablePropertyType),
                    ifFalse: nullablePropertyValue);
            }
            else
            {
                // need to cast this to nullable as EF would fail while materializing if the property is not nullable and source is null.
                propertyValue = nullablePropertyValue;
            }

            return propertyValue;
        }

        /// <summary>
        /// Returns an <see cref="Expression"/> that represents access to <paramref name="edmProperty"/>.
        /// </summary>
        /// <param name="source">The source that contains the <paramref name="edmProperty"/>.</param>
        /// <param name="edmProperty">The EDM property which access expression to return.</param>
        /// <returns>The property access <see cref="Expression"/>.</returns>
        protected virtual Expression CreatePropertyAccessExpression(Expression source, IEdmProperty edmProperty)
        {
            var propertyName = EdmLibHelpers.GetClrPropertyName(edmProperty, _model);
            PropertyInfo propertyInfo = source.Type.GetProperty(propertyName);
            return Expression.Property(source, propertyInfo);
        }

        /// <summary>
        /// Returns an <see cref="Expression"/> that represents access to property with name <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="source">The source that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property access <see cref="Expression"/>.</returns>
        protected virtual Expression CreatePropertyAccessExpression(Expression source, string propertyName)
        {
            return Expression.Property(source, propertyName);
        }

        private class ReferenceNavigationPropertyExpandFilterVisitor : ExpressionVisitor
        {
            private Expression _source;
            private ParameterExpression _parameterExpression;

            public ReferenceNavigationPropertyExpandFilterVisitor(ParameterExpression parameterExpression, Expression source)
            {
                _source = source;
                _parameterExpression = parameterExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node != _parameterExpression)
                {
                    throw new ODataException(Error.Format(SRResources.ReferenceNavigationPropertyExpandFilterVisitorUnexpectedParameter, node.Name));
                }

                return _source;
            }
        }

        // Generates the expression
        //      source => new Wrapper { Instance = source, Container = new PropertyContainer { ..expanded properties.. } }
        private Expression ProjectElement(Expression source, SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource)
        {
            Contract.Assert(source != null);

            Type elementType = source.Type;
            Type wrapperType = typeof(SelectExpandWrapper<>).MakeGenericType(elementType);
            List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

            PropertyInfo wrapperProperty;
            Expression wrapperPropertyValueExpression;
            bool isInstancePropertySet = false;
            bool isTypeNamePropertySet = false;
            bool isContainerPropertySet = false;

            // Initialize property 'ModelID' on the wrapper class.
            // source = new Wrapper { ModelID = 'some-guid-id' }
            wrapperProperty = wrapperType.GetProperty("ModelID");
            wrapperPropertyValueExpression = _settings.EnableConstantParameterization ?
                LinqParameterContainer.Parameterize(typeof(string), _modelID) :
                Expression.Constant(_modelID);
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, wrapperPropertyValueExpression));

            if (IsSelectAll(selectExpandClause))
            {
                // Initialize property 'Instance' on the wrapper class
                wrapperProperty = wrapperType.GetProperty("Instance");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, source));

                wrapperProperty = wrapperType.GetProperty("UseInstanceForProperties");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, Expression.Constant(true)));
                isInstancePropertySet = true;
            }
            else
            {
                // Initialize property 'TypeName' on the wrapper class as we don't have the instance.
                Expression typeName = CreateTypeNameExpression(source, structuredType, _model);
                if (typeName != null)
                {
                    isTypeNamePropertySet = true;
                    wrapperProperty = wrapperType.GetProperty("InstanceType");
                    wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, typeName));
                }
            }

            // Initialize the property 'Container' on the wrapper class
            // source => new Wrapper { Container =  new PropertyContainer { .... } }
            if (selectExpandClause != null)
            {
                Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand = GetPropertiesToExpandInQuery(selectExpandClause);
                ISet<IEdmStructuralProperty> autoSelectedProperties;

                ISet<IEdmStructuralProperty> propertiesToInclude = GetPropertiesToIncludeInQuery(selectExpandClause, structuredType, navigationSource, _model, out autoSelectedProperties);
                bool isSelectingOpenTypeSegments = GetSelectsOpenTypeSegments(selectExpandClause, structuredType);

                if (propertiesToExpand.Count > 0 || propertiesToInclude.Count > 0 || autoSelectedProperties.Count > 0 || isSelectingOpenTypeSegments)
                {
                    Expression propertyContainerCreation =
                        BuildPropertyContainer(structuredType, source, propertiesToExpand, propertiesToInclude, autoSelectedProperties, isSelectingOpenTypeSegments);

                    if (propertyContainerCreation != null)
                    {
                        wrapperProperty = wrapperType.GetProperty("Container");
                        Contract.Assert(wrapperProperty != null);

                        wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, propertyContainerCreation));
                        isContainerPropertySet = true;
                    }
                }
            }

            Type wrapperGenericType = GetWrapperGenericType(isInstancePropertySet, isTypeNamePropertySet, isContainerPropertySet);
            wrapperType = wrapperGenericType.MakeGenericType(elementType);
            return Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments);
        }

        private static bool GetSelectsOpenTypeSegments(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType)
        {
            if (structuredType == null || !structuredType.IsOpen)
            {
                return false;
            }

            if (IsSelectAll(selectExpandClause))
            {
                return true;
            }

            return selectExpandClause.SelectedItems.OfType<PathSelectItem>().Any(x => x.SelectedPath.LastSegment is DynamicPathSegment);
        }

        /// <summary>
        /// Returns an <see cref="Expression"/> that represents the count of items in <paramref name="source"/> if it is applicable.
        /// </summary>
        /// <param name="source">The source which items should be count.</param>
        /// <param name="expandItem">The <see cref="ExpandedReferenceSelectItem"/>.</param>
        /// <returns>The count <see cref="Expression"/>.</returns>
        protected virtual Expression CreateTotalCountExpression(Expression source, ExpandedReferenceSelectItem expandItem)
        {
            Expression countExpression = Expression.Constant(null, typeof(long?));
            if (expandItem.CountOption == null || !expandItem.CountOption.Value)
            {
                return countExpression;
            }

            Type elementType;
            if (!TypeHelper.IsCollection(source.Type, out elementType))
            {
                return countExpression;
            }

            MethodInfo countMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(elementType);
            }

            // call Count() method.
            countExpression = Expression.Call(null, countMethod, new[] { source });

            if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : countExpression
                return Expression.Condition(
                       test: Expression.Equal(source, Expression.Constant(null)),
                       ifTrue: Expression.Constant(null, typeof(long?)),
                       ifFalse: ExpressionHelpers.ToNullable(countExpression));
            }
            else
            {
                return countExpression;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        private Expression BuildPropertyContainer(IEdmStructuredType elementType, Expression source,
            Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand,
            ISet<IEdmStructuralProperty> propertiesToInclude, ISet<IEdmStructuralProperty> autoSelectedProperties, bool isSelectingOpenTypeSegments)
        {
            IList<NamedPropertyExpression> includedProperties = new List<NamedPropertyExpression>();

            foreach (KeyValuePair<IEdmNavigationProperty, ExpandedReferenceSelectItem> kvp in propertiesToExpand)
            {
                IEdmNavigationProperty propertyToExpand = kvp.Key;
                ExpandedReferenceSelectItem expandItem = kvp.Value;

                SelectExpandClause projection = GetOrCreateSelectExpandClause(kvp);

                ModelBoundQuerySettings querySettings = EdmLibHelpers.GetModelBoundQuerySettings(propertyToExpand,
                    propertyToExpand.ToEntityType(),
                    _context.Model);

                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToExpand, source);
                Expression propertyValue = CreatePropertyValueExpressionWithFilter(elementType, propertyToExpand, source,
                    expandItem);
                Expression nullCheck = GetNullCheckExpression(propertyToExpand, propertyValue, projection);

                Expression countExpression = CreateTotalCountExpression(propertyValue, expandItem);

                // projection can be null if the expanded navigation property is not further projected or expanded.
                if (projection != null)
                {
                    int? modelBoundPageSize = querySettings == null ? null : querySettings.PageSize;
                    propertyValue = ProjectAsWrapper(propertyValue, projection, propertyToExpand.ToEntityType(), expandItem.NavigationSource, expandItem, modelBoundPageSize);
                }

                NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
                if (projection != null)
                {
                    if (!propertyToExpand.Type.IsCollection())
                    {
                        propertyExpression.NullCheck = nullCheck;
                    }
                    else if (_settings.PageSize.HasValue)
                    {
                        propertyExpression.PageSize = _settings.PageSize.Value;
                    }
                    else
                    {
                        if (querySettings != null && querySettings.PageSize.HasValue)
                        {
                            propertyExpression.PageSize = querySettings.PageSize.Value;
                        }
                    }

                    propertyExpression.TotalCount = countExpression;
                    propertyExpression.CountOption = expandItem.CountOption;
                }

                includedProperties.Add(propertyExpression);
            }

            foreach (IEdmStructuralProperty propertyToInclude in propertiesToInclude)
            {
                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToInclude, source);
                Expression propertyValue = CreatePropertyValueExpression(elementType, propertyToInclude, source);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
            }

            foreach (IEdmStructuralProperty propertyToInclude in autoSelectedProperties)
            {
                Expression propertyName = CreatePropertyNameExpression(elementType, propertyToInclude, source);
                Expression propertyValue = CreatePropertyValueExpression(elementType, propertyToInclude, source);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue) { AutoSelected = true });
            }

            if (isSelectingOpenTypeSegments)
            {
                var dynamicPropertyDictionary = EdmLibHelpers.GetDynamicPropertyDictionary(elementType, _model);
                if (dynamicPropertyDictionary != null)
                {
                    Expression propertyName = Expression.Constant(dynamicPropertyDictionary.Name);
                    Expression propertyValue = CreatePropertyAccessExpression(source, dynamicPropertyDictionary.Name);
                    Expression nullablePropertyValue = ExpressionHelpers.ToNullable(propertyValue);
                    if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
                    {
                        // source == null ? null : propertyValue
                        propertyValue = Expression.Condition(
                            test: Expression.Equal(source, Expression.Constant(value: null)),
                            ifTrue: Expression.Constant(value: null, type: TypeHelper.ToNullable(propertyValue.Type)),
                            ifFalse: nullablePropertyValue);
                    }
                    else
                    {
                        propertyValue = nullablePropertyValue;
                    }

                    includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
                }
            }

            // create a property container that holds all these property names and values.
            return PropertyContainer.CreatePropertyContainer(includedProperties);
        }

        private static SelectExpandClause GetOrCreateSelectExpandClause(KeyValuePair<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertyToExpand)
        {
            // for normal $expand=....
            ExpandedNavigationSelectItem expandNavigationSelectItem = propertyToExpand.Value as ExpandedNavigationSelectItem;
            if (expandNavigationSelectItem != null)
            {
                return expandNavigationSelectItem.SelectAndExpand;
            }

            // for $expand=..../$ref, just includes the keys properties.
            IList<SelectItem> selectItems = new List<SelectItem>();
            foreach (IEdmStructuralProperty keyProperty in propertyToExpand.Key.ToEntityType().Key())
            {
                selectItems.Add(new PathSelectItem(new ODataSelectPath(new PropertySegment(keyProperty))));
            }

            return new SelectExpandClause(selectItems, false);
        }

        private Expression AddOrderByQueryForSource(Expression source, OrderByClause orderbyClause, Type elementType)
        {
            if (orderbyClause != null)
            {
                // TODO: Implement proper support for $select/$expand after $apply
                ODataQuerySettings querySettings = new ODataQuerySettings()
                {
                    HandleNullPropagation = HandleNullPropagationOption.True,
                };

                LambdaExpression orderByExpression =
                    FilterBinder.Bind(null, orderbyClause, elementType, _context, querySettings);
                source = ExpressionHelpers.OrderBy(source, orderByExpression, elementType, orderbyClause.Direction);
            }

            return source;
        }

        private Expression GetNullCheckExpression(IEdmNavigationProperty propertyToExpand, Expression propertyValue,
            SelectExpandClause projection)
        {
            if (projection == null || propertyToExpand.Type.IsCollection())
            {
                return null;
            }

            if (IsSelectAll(projection) || !propertyToExpand.ToEntityType().Key().Any())
            {
                return Expression.Equal(propertyValue, Expression.Constant(null));
            }

            Expression keysNullCheckExpression = null;
            foreach (var key in propertyToExpand.ToEntityType().Key())
            {
                var propertyValueExpression = CreatePropertyValueExpressionWithFilter(propertyToExpand.ToEntityType(), key, propertyValue, null);
                var keyExpression = Expression.Equal(
                    propertyValueExpression,
                    Expression.Constant(null, propertyValueExpression.Type));

                keysNullCheckExpression = keysNullCheckExpression == null
                    ? keyExpression
                    : Expression.And(keysNullCheckExpression, keyExpression);
            }

            return keysNullCheckExpression;
        }

        // new CollectionWrapper<ElementType> { Instance = source.Select((ElementType element) => new Wrapper { }) }
        private Expression ProjectCollection(Expression source, Type elementType, SelectExpandClause selectExpandClause, IEdmEntityType entityType, IEdmNavigationSource navigationSource, ExpandedReferenceSelectItem expandedItem, int? modelBoundPageSize)
        {
            ParameterExpression element = Expression.Parameter(elementType);

            // expression
            //      new Wrapper { }
            Expression projection = ProjectElement(element, selectExpandClause, entityType, navigationSource);

            // expression
            //      (ElementType element) => new Wrapper { }
            LambdaExpression selector = Expression.Lambda(projection, element);

            if (expandedItem != null)
            {
                source = AddOrderByQueryForSource(source, expandedItem.OrderByOption, elementType);
            }

            if (_settings.PageSize.HasValue || modelBoundPageSize.HasValue ||
                (expandedItem != null && (expandedItem.TopOption.HasValue || expandedItem.SkipOption.HasValue)))
            {
                // nested paging. Need to apply order by first, and take one more than page size as we need to know
                // whether the collection was truncated or not while generating next page links.
                IEnumerable<IEdmStructuralProperty> properties =
                    entityType.Key().Any()
                        ? entityType.Key()
                        : entityType
                            .StructuralProperties()
                            .Where(property => property.Type.IsPrimitive() && !property.Type.IsStream())
                            .OrderBy(property => property.Name);

                if (expandedItem == null || expandedItem.OrderByOption == null)
                {
                    bool alreadyOrdered = false;
                    foreach (var prop in properties)
                    {
                        source = ExpressionHelpers.OrderByPropertyExpression(source, prop.Name, elementType,
                            alreadyOrdered);
                        if (!alreadyOrdered)
                        {
                            alreadyOrdered = true;
                        }
                    }
                }

                if (expandedItem != null && expandedItem.SkipOption.HasValue)
                {
                    Contract.Assert(expandedItem.SkipOption.Value <= Int32.MaxValue);
                    source = ExpressionHelpers.Skip(source, (int)expandedItem.SkipOption.Value, elementType,
                        _settings.EnableConstantParameterization);
                }

                if (expandedItem != null && expandedItem.TopOption.HasValue)
                {
                    Contract.Assert(expandedItem.TopOption.Value <= Int32.MaxValue);
                    source = ExpressionHelpers.Take(source, (int)expandedItem.TopOption.Value, elementType,
                        _settings.EnableConstantParameterization);
                }

                // don't page nested collections if EnableCorrelatedSubqueryBuffering is enabled
                if (expandedItem == null || !_settings.EnableCorrelatedSubqueryBuffering)
                {
                    if (_settings.PageSize.HasValue)
                    {
                        source = ExpressionHelpers.Take(source, _settings.PageSize.Value + 1, elementType,
                            _settings.EnableConstantParameterization);
                    }
                    else if (_settings.ModelBoundPageSize.HasValue)
                    {
                        source = ExpressionHelpers.Take(source, modelBoundPageSize.Value + 1, elementType,
                            _settings.EnableConstantParameterization);
                    }
                }
            }

            // expression
            //      source.Select((ElementType element) => new Wrapper { })
            var selectMethod = GetSelectMethod(elementType, projection.Type);
            Expression selectedExpresion = Expression.Call(selectMethod, source, selector);

            // Append ToList() to collection as a hint to LINQ provider to buffer correlated subqueries in memory and avoid executing N+1 queries
            if (_settings.EnableCorrelatedSubqueryBuffering)
            {
                selectedExpresion = Expression.Call(ExpressionHelperMethods.QueryableToList.MakeGenericMethod(projection.Type), selectedExpresion);
            }

            if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : projectedCollection
                return Expression.Condition(
                       test: Expression.Equal(source, Expression.Constant(null)),
                       ifTrue: Expression.Constant(null, selectedExpresion.Type),
                       ifFalse: selectedExpresion);
            }
            else
            {
                return selectedExpresion;
            }
        }

        // OData formatter requires the type name of the entity that is being written if the type has derived types.
        // Expression
        //      source is GrandChild ? "GrandChild" : ( source is Child ? "Child" : "Root" )
        // Notice that the order is important here. The most derived type must be the first to check.
        // If entity framework had a way to figure out the type name without selecting the whole object, we don't have to do this magic.
        internal static Expression CreateTypeNameExpression(Expression source, IEdmStructuredType elementType, IEdmModel model)
        {
            IReadOnlyList<IEdmStructuredType> derivedTypes = GetAllDerivedTypes(elementType, model);
            if (derivedTypes.Count == 0)
            {
                // no inheritance.
                return null;
            }
            else
            {
                Expression expression = Expression.Constant(elementType.FullTypeName());
                for (int i = 0; i < derivedTypes.Count; i++)
                {
                    Type clrType = EdmLibHelpers.GetClrType(derivedTypes[i], model);
                    if (clrType == null)
                    {
                        throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, derivedTypes[0].FullTypeName()));
                    }

                    expression = Expression.Condition(
                                    test: Expression.TypeIs(source, clrType),
                                    ifTrue: Expression.Constant(derivedTypes[i].FullTypeName()),
                                    ifFalse: expression);
                }

                return expression;
            }
        }

        // returns all the derived types (direct and indirect) of baseType ordered according to their depth. The direct children
        // are the first in the list.
        private static IReadOnlyList<IEdmStructuredType> GetAllDerivedTypes(IEdmStructuredType baseType, IEdmModel model)
        {
            IEnumerable<IEdmStructuredType> allStructuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();

            List<Tuple<int, IEdmStructuredType>> derivedTypes = new List<Tuple<int, IEdmStructuredType>>();
            foreach (IEdmStructuredType structuredType in allStructuredTypes)
            {
                int distance = IsDerivedTypeOf(structuredType, baseType);
                if (distance > 0)
                {
                    derivedTypes.Add(Tuple.Create(distance, structuredType));
                }
            }

            return derivedTypes.OrderBy(tuple => tuple.Item1).Select(tuple => tuple.Item2).ToList();
        }

        // returns -1 if type does not derive from baseType and a positive number representing the distance
        // between them if it does.
        private static int IsDerivedTypeOf(IEdmStructuredType type, IEdmStructuredType baseType)
        {
            int distance = 0;
            while (type != null)
            {
                if (baseType == type)
                {
                    return distance;
                }

                type = type.BaseType();
                distance++;
            }

            return -1;
        }

        private static MethodInfo GetSelectMethod(Type elementType, Type resultType)
        {
            return ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(elementType, resultType);
        }

        private static Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> GetPropertiesToExpandInQuery(SelectExpandClause selectExpandClause)
        {
            Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> properties = new Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem>();

            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                ExpandedReferenceSelectItem expandItem = selectItem as ExpandedReferenceSelectItem;
                if (expandItem != null)
                {
                    SelectExpandNode.ValidatePathIsSupportedForExpand(expandItem.PathToNavigationProperty);
                    NavigationPropertySegment navigationSegment = expandItem.PathToNavigationProperty.LastSegment as NavigationPropertySegment;
                    if (navigationSegment == null)
                    {
                        throw new ODataException(SRResources.UnsupportedSelectExpandPath);
                    }

                    properties[navigationSegment.NavigationProperty] = expandItem;
                }
            }

            return properties;
        }

        private static ISet<IEdmStructuralProperty> GetPropertiesToIncludeInQuery(
            SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource, IEdmModel model, out ISet<IEdmStructuralProperty> autoSelectedProperties)
        {
            IEdmEntityType entityType = structuredType as IEdmEntityType;

            autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
            HashSet<IEdmStructuralProperty> propertiesToInclude = new HashSet<IEdmStructuralProperty>();

            IEnumerable<SelectItem> selectedItems = selectExpandClause.SelectedItems;
            if (!IsSelectAll(selectExpandClause))
            {
                // only select requested properties and keys.
                foreach (PathSelectItem pathSelectItem in selectedItems.OfType<PathSelectItem>())
                {
                    SelectExpandNode.ValidatePathIsSupportedForSelect(pathSelectItem.SelectedPath);
                    PropertySegment structuralPropertySegment = pathSelectItem.SelectedPath.LastSegment as PropertySegment;
                    if (structuralPropertySegment != null)
                    {
                        propertiesToInclude.Add(structuralPropertySegment.Property);
                    }
                }

                foreach (ExpandedNavigationSelectItem expandedNavigationSelectItem in selectedItems.OfType<ExpandedNavigationSelectItem>())
                {
                    foreach (var segment in expandedNavigationSelectItem.PathToNavigationProperty)
                    {
                        PropertySegment propertySegment = segment as PropertySegment;
                        if (propertySegment != null &&
                            structuredType.Properties().Contains(propertySegment.Property))
                        {
                            propertiesToInclude.Add(propertySegment.Property);
                            break;
                        }
                    }
                }

                if (entityType != null)
                {
                    // add keys
                    foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                    {
                        if (!propertiesToInclude.Contains(keyProperty))
                        {
                            autoSelectedProperties.Add(keyProperty);
                        }
                    }
                }

                // add concurrency properties, if not added
                if (navigationSource != null && model != null)
                {
                    IEnumerable<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(navigationSource);
                    foreach (IEdmStructuralProperty concurrencyProperty in concurrencyProperties)
                    {
                        if (!propertiesToInclude.Contains(concurrencyProperty))
                        {
                            autoSelectedProperties.Add(concurrencyProperty);
                        }
                    }
                }
            }

            return propertiesToInclude;
        }

        private static bool IsSelectAll(SelectExpandClause selectExpandClause)
        {
            if (selectExpandClause == null)
            {
                return true;
            }

            if (selectExpandClause.AllSelected || selectExpandClause.SelectedItems.OfType<WildcardSelectItem>().Any())
            {
                return true;
            }

            return false;
        }

        private static Type GetWrapperGenericType(bool isInstancePropertySet, bool isTypeNamePropertySet, bool isContainerPropertySet)
        {
            if (isInstancePropertySet)
            {
                // select all
                Contract.Assert(!isTypeNamePropertySet, "we don't set type name if we set instance as it can be figured from instance");

                return isContainerPropertySet ? typeof(SelectAllAndExpand<>) : typeof(SelectAll<>);
            }
            else
            {
                Contract.Assert(isContainerPropertySet, "if it is not select all, container should hold something");

                return isTypeNamePropertySet ? typeof(SelectSomeAndInheritance<>) : typeof(SelectSome<>);
            }
        }

        /* Entityframework requires that the two different type initializers for a given type in the same query have the
        same set of properties in the same order.

        A ~/People?$select=Name&$expand=Friend results in a select expression that has two SelectExpandWrapper<Person>
        expressions, one for the root level person and the second for the expanded Friend person.
        The first wrapper has the Container property set (contains Name and Friend values) where as the second wrapper
        has the Instance property set as it contains all the properties of the expanded person.

        The below four classes workaround that entity framework limitation by defining a seperate type for each
        property selection combination possible. */

        private class SelectAllAndExpand<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectAll<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectSomeAndInheritance<TEntity> : SelectExpandWrapper<TEntity>
        {
        }

        private class SelectSome<TEntity> : SelectAllAndExpand<TEntity>
        {
        }
    }
}
