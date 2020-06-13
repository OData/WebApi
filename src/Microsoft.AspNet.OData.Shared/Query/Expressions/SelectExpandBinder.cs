// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Applies the given <see cref="SelectExpandQueryOption"/> to the given <see cref="IQueryable"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable.")]
    internal class SelectExpandBinder
    {
        private ODataQueryContext _context;
        private IEdmModel _model;
        private ODataQuerySettings _settings;
        private string _modelID;

        public SelectExpandBinder(ODataQuerySettings settings, ODataQueryContext context)
        {
            Contract.Assert(settings != null);
            Contract.Assert(context != null);
            Contract.Assert(context.Model != null);
            Contract.Assert(settings.HandleNullPropagation != HandleNullPropagationOption.Default);

            _context = context;
            _model = _context.Model;
            _modelID = ModelContainer.GetModelID(_model);
            _settings = settings;
        }

        public static IQueryable Bind(IQueryable queryable, ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(queryable != null);
            Contract.Assert(selectExpandQuery != null);

            SelectExpandBinder binder = new SelectExpandBinder(settings, selectExpandQuery.Context);
            return binder.Bind(queryable, selectExpandQuery);
        }

        public static object Bind(object entity, ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            Contract.Assert(entity != null);
            Contract.Assert(selectExpandQuery != null);

            SelectExpandBinder binder = new SelectExpandBinder(settings, selectExpandQuery.Context);
            return binder.Bind(entity, selectExpandQuery);
        }

        private object Bind(object entity, SelectExpandQueryOption selectExpandQuery)
        {
            // Needn't to verify the input, that's done at upper level.
            LambdaExpression projectionLambda = GetProjectionLambda(selectExpandQuery);

            // TODO: cache this ?
            return projectionLambda.Compile().DynamicInvoke(entity);
        }

        private IQueryable Bind(IQueryable queryable, SelectExpandQueryOption selectExpandQuery)
        {
            // Needn't to verify the input, that's done at upper level.
            Type elementType = selectExpandQuery.Context.ElementClrType;

            LambdaExpression projectionLambda = GetProjectionLambda(selectExpandQuery);

            MethodInfo selectMethod = ExpressionHelperMethods.QueryableSelectGeneric.MakeGenericMethod(elementType, projectionLambda.Body.Type);
            return selectMethod.Invoke(null, new object[] { queryable, projectionLambda }) as IQueryable;
        }

        private LambdaExpression GetProjectionLambda(SelectExpandQueryOption selectExpandQuery)
        {
            Type elementType = selectExpandQuery.Context.ElementClrType;
            IEdmNavigationSource navigationSource = selectExpandQuery.Context.NavigationSource;
            ParameterExpression source = Expression.Parameter(elementType, "$it");

            // expression looks like -> new Wrapper { Instance = source , Properties = "...", Container = new PropertyContainer { ... } }
            Expression projectionExpression = ProjectElement(source, selectExpandQuery.SelectExpandClause, _context.ElementType as IEdmStructuredType, navigationSource);

            // expression looks like -> source => new Wrapper { Instance = source .... }
            LambdaExpression projectionLambdaExpression = Expression.Lambda(projectionExpression, source);

            return projectionLambdaExpression;
        }

        internal Expression ProjectAsWrapper(Expression source, SelectExpandClause selectExpandClause,
            IEdmStructuredType structuredType, IEdmNavigationSource navigationSource, OrderByClause orderByClause = null,
            long? topOption = null,
            long? skipOption = null,
            int? modelBoundPageSize = null)
        {
            Type elementType;
            if (TypeHelper.IsCollection(source.Type, out elementType))
            {
                // new CollectionWrapper<ElementType> { Instance = source.Select(s => new Wrapper { ... }) };
                return ProjectCollection(source, elementType, selectExpandClause, structuredType, navigationSource, orderByClause,
                    topOption,
                    skipOption,
                    modelBoundPageSize);
            }
            else
            {
                // new Wrapper { v1 = source.property ... }
                return ProjectElement(source, selectExpandClause, structuredType, navigationSource);
            }
        }

        internal Expression CreatePropertyNameExpression(IEdmStructuredType elementType, IEdmProperty property, Expression source)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(property != null);
            Contract.Assert(source != null);

            IEdmStructuredType declaringType = property.DeclaringType;

            // derived property using cast
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
                        ifTrue: Expression.Constant(property.Name),
                        ifFalse: Expression.Constant(null, typeof(string)));
                }
            }

            // Expression
            //          "propertyName"
            return Expression.Constant(property.Name);
        }

        internal Expression CreatePropertyValueExpression(IEdmStructuredType elementType, IEdmProperty property, Expression source, FilterClause filterClause)
        {
            Contract.Assert(elementType != null);
            Contract.Assert(property != null);
            Contract.Assert(source != null);

            // Expression: source = source as propertyDeclaringType
            if (elementType != property.DeclaringType)
            {
                Type castType = EdmLibHelpers.GetClrType(property.DeclaringType, _model);
                if (castType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, property.DeclaringType.FullTypeName()));
                }

                source = Expression.TypeAs(source, castType);
            }

            // Expression:  source.Property
            string propertyName = EdmLibHelpers.GetClrPropertyName(property, _model);
            PropertyInfo propertyInfo = source.Type.GetProperty(propertyName);
            Expression propertyValue = Expression.Property(source, propertyInfo);
            Type nullablePropertyType = TypeHelper.ToNullable(propertyValue.Type);
            Expression nullablePropertyValue = ExpressionHelpers.ToNullable(propertyValue);

            if (filterClause != null)
            {
                bool isCollection = property.Type.IsCollection();

                IEdmTypeReference edmElementType = (isCollection ? property.Type.AsCollection().ElementType() : property.Type);
                Type clrElementType = EdmLibHelpers.GetClrType(edmElementType, _model);
                if (clrElementType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, edmElementType.FullName()));
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
                        throw new ODataException(Error.Format(SRResources.ExpandFilterExpressionNotLambdaExpression, property.Name, "LambdaExpression"));
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

        // Generates the expression
        //      source => new Wrapper { Instance = source, Container = new PropertyContainer { ..expanded properties.. } }
        internal Expression ProjectElement(Expression source, SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource)
        {
            Contract.Assert(source != null);

            // If it's not a structural type, just return the source.
            if (structuredType == null)
            {
                return source;
            }

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
                IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
                IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
                ISet<IEdmStructuralProperty> autoSelectedProperties;

                bool isContainDynamicPropertySelection = GetSelectExpandProperties(_model, structuredType, navigationSource, selectExpandClause,
                    out propertiesToInclude,
                    out propertiesToExpand,
                    out autoSelectedProperties);

                bool isSelectingOpenTypeSegments = isContainDynamicPropertySelection || IsSelectAllOnOpenType(selectExpandClause, structuredType);

                if (propertiesToExpand != null || propertiesToInclude != null || autoSelectedProperties != null || isSelectingOpenTypeSegments)
                {
                    Expression propertyContainerCreation =
                        BuildPropertyContainer(source, structuredType, propertiesToExpand, propertiesToInclude, autoSelectedProperties, isSelectingOpenTypeSegments);

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

        /// <summary>
        /// Gets the $select and $expand properties from the given <see cref="SelectExpandClause"/>
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="structuredType">The current structural type.</param>
        /// <param name="navigationSource">The current navigation source.</param>
        /// <param name="selectExpandClause">The given select and expand clause.</param>
        /// <param name="propertiesToInclude">The out properties to include at current level, could be null.</param>
        /// <param name="propertiesToExpand">The out properties to expand at current level, could be null.</param>
        /// <param name="autoSelectedProperties">The out auto selected properties to include at current level, could be null.</param>
        /// <returns>true if the select contains dynamic property selection, false if it's not.</returns>
        internal static bool GetSelectExpandProperties(IEdmModel model, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource,
            SelectExpandClause selectExpandClause,
            out IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude,
            out IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand,
            out ISet<IEdmStructuralProperty> autoSelectedProperties)
        {
            Contract.Assert(selectExpandClause != null);

            // Properties to be included includes all the properties selected or in the middle of a $select and $expand path.
            // for example: "$expand=abc/xyz/nav", "abc" and "xyz" are the middle properties that should be included.
            // meanwhile, "nav" is the property that should be expanded.
            // If it's a type cast path, for example: $select=NS.TypeCast/abc, "abc" should be included also.
            propertiesToInclude = null;
            propertiesToExpand = null;
            autoSelectedProperties = null;

            bool isSelectContainsDynamicProperty = false;
            var currentLevelPropertiesInclude = new Dictionary<IEdmStructuralProperty, SelectExpandIncludedProperty>();
            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                // $expand=...
                ExpandedReferenceSelectItem expandedItem = selectItem as ExpandedReferenceSelectItem;
                if (expandedItem != null)
                {
                    ProcessExpandedItem(expandedItem, navigationSource, currentLevelPropertiesInclude, ref propertiesToExpand);
                    continue;
                }

                // $select=...
                PathSelectItem pathItem = selectItem as PathSelectItem;
                if (pathItem != null)
                {
                    if (ProcessSelectedItem(pathItem, navigationSource, currentLevelPropertiesInclude))
                    {
                        isSelectContainsDynamicProperty = true;
                    }
                    continue;
                }

                // Skip processing the "WildcardSelectItem and NamespaceQualifiedWildcardSelectItem"
                // ODL now doesn't support "$select=property/*" and "$select=property/NS.*"
            }

            if (!IsSelectAll(selectExpandClause))
            {
                // We should include the keys if it's an entity.
                IEdmEntityType entityType = structuredType as IEdmEntityType;
                if (entityType != null)
                {
                    foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                    {
                        if (!currentLevelPropertiesInclude.Keys.Contains(keyProperty))
                        {
                            if (autoSelectedProperties == null)
                            {
                                autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
                            }

                            autoSelectedProperties.Add(keyProperty);
                        }
                    }
                }

                // We should add concurrency properties, if not added
                if (navigationSource != null && model != null)
                {
                    IEnumerable<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(navigationSource);
                    foreach (IEdmStructuralProperty concurrencyProperty in concurrencyProperties)
                    {
                        if (structuredType.Properties().Any(p => p == concurrencyProperty))
                        {
                            if (!currentLevelPropertiesInclude.Keys.Contains(concurrencyProperty))
                            {
                                if (autoSelectedProperties == null)
                                {
                                    autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
                                }

                                autoSelectedProperties.Add(concurrencyProperty);
                            }
                        }
                    }
                }
            }

            if (currentLevelPropertiesInclude.Any())
            {
                propertiesToInclude = new Dictionary<IEdmStructuralProperty, PathSelectItem>();
                foreach (var propertiesInclude in currentLevelPropertiesInclude)
                {
                    propertiesToInclude[propertiesInclude.Key] = propertiesInclude.Value == null ? null : propertiesInclude.Value.ToPathSelectItem();
                }
            }

            return isSelectContainsDynamicProperty;
        }

        /// <summary>
        /// Process the <see cref="ExpandedReferenceSelectItem"/>.
        /// </summary>
        /// <param name="expandedItem">The expaned item.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="currentLevelPropertiesInclude">The current level properties included.</param>
        /// <param name="propertiesToExpand">out/ref, the property expanded.</param>
        private static void ProcessExpandedItem(ExpandedReferenceSelectItem expandedItem,
            IEdmNavigationSource navigationSource,
            IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude,
            ref IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand)
        {
            Contract.Assert(expandedItem != null && expandedItem.PathToNavigationProperty != null);
            Contract.Assert(currentLevelPropertiesInclude != null);

            // Verify and process the $expand=... path.
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = expandedItem.PathToNavigationProperty.GetFirstNonTypeCastSegment(out remainingSegments);

            // for $expand=NS.SubType/Nav, we don't care about the leading type segment, because with or without the type segment
            // the "nav" property value expression should be built into the property container.

            PropertySegment firstStructuralPropertySegment = firstNonTypeSegment as PropertySegment;
            if (firstStructuralPropertySegment != null)
            {
                // for example: $expand=abc/nav, the remaining segments should never be null because at least the last navigation segment is there.
                Contract.Assert(remainingSegments != null);

                SelectExpandIncludedProperty newPropertySelectItem;
                if (!currentLevelPropertiesInclude.TryGetValue(firstStructuralPropertySegment.Property, out newPropertySelectItem))
                {
                    newPropertySelectItem = new SelectExpandIncludedProperty(firstStructuralPropertySegment, navigationSource);
                    currentLevelPropertiesInclude[firstStructuralPropertySegment.Property] = newPropertySelectItem;
                }

                newPropertySelectItem.AddSubExpandItem(remainingSegments, expandedItem);
            }
            else
            {
                // for example: $expand=nav, if we couldn't find a structural property in the path, it means we get the last navigation segment.
                // So, the remaing segments should be null and the last segment should be "NavigationPropertySegment".
                Contract.Assert(remainingSegments == null);

                NavigationPropertySegment firstNavigationPropertySegment = firstNonTypeSegment as NavigationPropertySegment;
                Contract.Assert(firstNavigationPropertySegment != null);

                // Needn't add this navigation property into the include property.
                // Because this navigation property will be included separately.
                if (propertiesToExpand == null)
                {
                    propertiesToExpand = new Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem>();
                }

                propertiesToExpand[firstNavigationPropertySegment.NavigationProperty] = expandedItem;
            }
        }

        /// <summary>
        /// Process the <see cref="PathSelectItem"/>.
        /// </summary>
        /// <param name="pathSelectItem">The selected item.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="currentLevelPropertiesInclude">The current level properties included.</param>
        /// <returns>true if it's dynamic property selection, false if it's not.</returns>
        private static bool ProcessSelectedItem(PathSelectItem pathSelectItem,
            IEdmNavigationSource navigationSource,
            IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude)
        {
            Contract.Assert(pathSelectItem != null && pathSelectItem.SelectedPath != null);
            Contract.Assert(currentLevelPropertiesInclude != null);

            // Verify and process the $select path
            IList<ODataPathSegment> remainingSegments;
            ODataPathSegment firstNonTypeSegment = pathSelectItem.SelectedPath.GetFirstNonTypeCastSegment(out remainingSegments);

            // for $select=NS.SubType/Property, we don't care about the leading type segment, because with or without the type segment
            // the "Property" property value expression should be built into the property container.

            PropertySegment firstSturucturalPropertySegment = firstNonTypeSegment as PropertySegment;
            if (firstSturucturalPropertySegment != null)
            {
                // $select=abc/..../xyz
                SelectExpandIncludedProperty newPropertySelectItem;
                if (!currentLevelPropertiesInclude.TryGetValue(firstSturucturalPropertySegment.Property, out newPropertySelectItem))
                {
                    newPropertySelectItem = new SelectExpandIncludedProperty(firstSturucturalPropertySegment, navigationSource);
                    currentLevelPropertiesInclude[firstSturucturalPropertySegment.Property] = newPropertySelectItem;
                }

                newPropertySelectItem.AddSubSelectItem(remainingSegments, pathSelectItem);
            }
            else
            {
                // If we can't find a PropertySegment, the $select path maybe selecting an operation, a navigation or dynamic property.
                // And the remaing segments should be null.
                Contract.Assert(remainingSegments == null);

                // For operation (action/function), needn't process it.
                // For navigation property, needn't process it here.

                // For dynamic property, let's test the last segment for this path select item.
                if (firstNonTypeSegment is DynamicPathSegment)
                {
                    return true;
                }
            }

            return false;
        }

        // To test whether the currect selection is SelectAll on an open type
        private static bool IsSelectAllOnOpenType(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType)
        {
            if (structuredType == null || !structuredType.IsOpen)
            {
                return false;
            }

            if (IsSelectAll(selectExpandClause))
            {
                return true;
            }

            return false;
        }

        private Expression CreateTotalCountExpression(Expression source, bool? countOption)
        {
            Expression countExpression = Expression.Constant(null, typeof(long?));
            if (countOption == null || !countOption.Value)
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

        private Expression BuildPropertyContainer(Expression source, IEdmStructuredType structuredType,
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand,
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude,
            ISet<IEdmStructuralProperty> autoSelectedProperties,
            bool isSelectingOpenTypeSegments)
        {
            IList<NamedPropertyExpression> includedProperties = new List<NamedPropertyExpression>();

            if (propertiesToExpand != null)
            {
                foreach (var propertyToExpand in propertiesToExpand)
                {
                    // $expand=abc or $expand=abc/$ref
                    BuildExpandedProperty(source, structuredType, propertyToExpand.Key, propertyToExpand.Value, includedProperties);
                }
            }

            if (propertiesToInclude != null)
            {
                foreach (var propertyToInclude in propertiesToInclude)
                {
                    // $select=abc($select=...,$filter=...,$compute=...)....
                    BuildSelectedProperty(source, structuredType, propertyToInclude.Key, propertyToInclude.Value, includedProperties);
                }
            }

            if (autoSelectedProperties != null)
            {
                foreach (IEdmStructuralProperty propertyToInclude in autoSelectedProperties)
                {
                    Expression propertyName = CreatePropertyNameExpression(structuredType, propertyToInclude, source);
                    Expression propertyValue = CreatePropertyValueExpression(structuredType, propertyToInclude, source, filterClause: null);
                    includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue)
                    {
                        AutoSelected = true
                    });
                }
            }

            if (isSelectingOpenTypeSegments)
            {
                BuildDynamicProperty(source, structuredType, includedProperties);
            }

            // create a property container that holds all these property names and values.
            return PropertyContainer.CreatePropertyContainer(includedProperties);
        }

        /// <summary>
        /// Build the navigation property <see cref="IEdmNavigationProperty"/> into the included properties.
        /// The property name is the navigation property name.
        /// The property value is the navigation property value from the source and applied the nested query options.
        /// </summary>
        /// <param name="source">The source contains the navigation property.</param>
        /// <param name="structuredType">The structured type or its derived type contains the navigation property.</param>
        /// <param name="navigationProperty">The expanded navigation property.</param>
        /// <param name="expandedItem">The expanded navigation select item. It may contain the neste query options.</param>
        /// <param name="includedProperties">The container to hold the created property.</param>
        internal void BuildExpandedProperty(Expression source, IEdmStructuredType structuredType,
            IEdmNavigationProperty navigationProperty, ExpandedReferenceSelectItem expandedItem,
            IList<NamedPropertyExpression> includedProperties)
        {
            Contract.Assert(source != null);
            Contract.Assert(structuredType != null);
            Contract.Assert(navigationProperty != null);
            Contract.Assert(expandedItem != null);
            Contract.Assert(includedProperties != null);

            IEdmEntityType edmEntityType = navigationProperty.ToEntityType();

            ModelBoundQuerySettings querySettings = EdmLibHelpers.GetModelBoundQuerySettings(navigationProperty, edmEntityType, _model);

            // TODO: Process $apply and $compute in the $expand here, will support later.
            // $apply=...; $compute=...

            // Expression:
            //       "navigation property name"
            Expression propertyName = CreatePropertyNameExpression(structuredType, navigationProperty, source);

            // Expression:
            //        source.NavigationProperty
            Expression propertyValue = CreatePropertyValueExpression(structuredType, navigationProperty, source, expandedItem.FilterOption);

            // Sub select and expand could be null if the expanded navigation property is not further projected or expanded.
            SelectExpandClause subSelectExpandClause = GetOrCreateSelectExpandClause(navigationProperty, expandedItem);

            Expression nullCheck = GetNullCheckExpression(navigationProperty, propertyValue, subSelectExpandClause);

            Expression countExpression = CreateTotalCountExpression(propertyValue, expandedItem.CountOption);

            int? modelBoundPageSize = querySettings == null ? null : querySettings.PageSize;
            propertyValue = ProjectAsWrapper(propertyValue, subSelectExpandClause, edmEntityType, expandedItem.NavigationSource,
                expandedItem.OrderByOption, // $orderby=...
                expandedItem.TopOption, // $top=...
                expandedItem.SkipOption, // $skip=...
                modelBoundPageSize);

            NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
            if (subSelectExpandClause != null)
            {
                if (!navigationProperty.Type.IsCollection())
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
                propertyExpression.CountOption = expandedItem.CountOption;
            }

            includedProperties.Add(propertyExpression);
        }

        /// <summary>
        /// Build the structural property <see cref="IEdmStructuralProperty"/> into the included properties.
        /// The property name is the structural property name.
        /// The property value is the structural property value from the source and applied the nested query options.
        /// </summary>
        /// <param name="source">The source contains the structural property.</param>
        /// <param name="structuredType">The structured type or its derived type contains the structural property.</param>
        /// <param name="structuralProperty">The selected structural property.</param>
        /// <param name="pathSelectItem">The selected item. It may contain the neste query options and could be null.</param>
        /// <param name="includedProperties">The container to hold the created property.</param>
        internal void BuildSelectedProperty(Expression source, IEdmStructuredType structuredType,
            IEdmStructuralProperty structuralProperty, PathSelectItem pathSelectItem,
            IList<NamedPropertyExpression> includedProperties)
        {
            Contract.Assert(source != null);
            Contract.Assert(structuredType != null);
            Contract.Assert(structuralProperty != null);
            Contract.Assert(includedProperties != null);

            // // Expression:
            //       "navigation property name"
            Expression propertyName = CreatePropertyNameExpression(structuredType, structuralProperty, source);

            // Expression:
            //        source.NavigationProperty
            Expression propertyValue;
            if (pathSelectItem == null)
            {
                propertyValue = CreatePropertyValueExpression(structuredType, structuralProperty, source, filterClause: null);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
                return;
            }

            SelectExpandClause subSelectExpandClause = pathSelectItem.SelectAndExpand;

            // TODO: Process $compute in the $select ahead.
            // $compute=...

            propertyValue = CreatePropertyValueExpression(structuredType, structuralProperty, source, pathSelectItem.FilterOption);
            Type propertyValueType = propertyValue.Type;
            if (propertyValueType == typeof(char[]) || propertyValueType == typeof(byte[]))
            {
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
                return;
            }

            Expression nullCheck = GetNullCheckExpression(structuralProperty, propertyValue, subSelectExpandClause);

            Expression countExpression = CreateTotalCountExpression(propertyValue, pathSelectItem.CountOption);

            // be noted: the property structured type could be null, because the property maybe not a complex property.
            IEdmStructuredType propertyStructuredType = structuralProperty.Type.ToStructuredType();
            ModelBoundQuerySettings querySettings = null;
            if (propertyStructuredType != null)
            {
                querySettings = EdmLibHelpers.GetModelBoundQuerySettings(structuralProperty, propertyStructuredType, _context.Model);
            }

            int? modelBoundPageSize = querySettings == null ? null : querySettings.PageSize;
            propertyValue = ProjectAsWrapper(propertyValue, subSelectExpandClause, structuralProperty.Type.ToStructuredType(), pathSelectItem.NavigationSource,
                pathSelectItem.OrderByOption, // $orderby=...
                pathSelectItem.TopOption, // $top=...
                pathSelectItem.SkipOption, // $skip=...
                modelBoundPageSize);

            NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
            if (subSelectExpandClause != null)
            {
                if (!structuralProperty.Type.IsCollection())
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
                propertyExpression.CountOption = pathSelectItem.CountOption;
            }

            includedProperties.Add(propertyExpression);
        }

        /// <summary>
        /// Build the dynamic properties into the included properties.
        /// </summary>
        /// <param name="source">The source contains the dynamic property.</param>
        /// <param name="structuredType">The structured type contains the dynamic property.</param>
        /// <param name="includedProperties">The container to hold the created property.</param>
        internal void BuildDynamicProperty(Expression source, IEdmStructuredType structuredType,
            IList<NamedPropertyExpression> includedProperties)
        {
            Contract.Assert(source != null);
            Contract.Assert(structuredType != null);
            Contract.Assert(includedProperties != null);

            PropertyInfo dynamicPropertyDictionary = EdmLibHelpers.GetDynamicPropertyDictionary(structuredType, _model);
            if (dynamicPropertyDictionary != null)
            {
                Expression propertyName = Expression.Constant(dynamicPropertyDictionary.Name);
                Expression propertyValue = Expression.Property(source, dynamicPropertyDictionary.Name);
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

        private static SelectExpandClause GetOrCreateSelectExpandClause(IEdmNavigationProperty navigationProperty, ExpandedReferenceSelectItem expandedItem)
        {
            // for normal $expand=....
            ExpandedNavigationSelectItem expandNavigationSelectItem = expandedItem as ExpandedNavigationSelectItem;
            if (expandNavigationSelectItem != null)
            {
                return expandNavigationSelectItem.SelectAndExpand;
            }

            // for $expand=..../$ref, just includes the keys properties
            IList<SelectItem> selectItems = new List<SelectItem>();
            foreach (IEdmStructuralProperty keyProperty in navigationProperty.ToEntityType().Key())
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

        private static Expression GetNullCheckExpression(IEdmStructuralProperty propertyToInclude, Expression propertyValue,
            SelectExpandClause projection)
        {
            if (projection == null || propertyToInclude.Type.IsCollection())
            {
                return null;
            }

            if (IsSelectAll(projection) && propertyToInclude.Type.IsComplex())
            {
                // for Collections (Primitive, Enum, Complex collection), that's check above.
                return Expression.Equal(propertyValue, Expression.Constant(null));
            }

            return null;
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
                var propertyValueExpression = CreatePropertyValueExpression(propertyToExpand.ToEntityType(), key, propertyValue, filterClause: null);
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
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        private Expression ProjectCollection(Expression source, Type elementType,
            SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource,
            OrderByClause orderByClause,
            long? topOption,
            long? skipOption,
            int? modelBoundPageSize)
        {
            // structuralType could be null, because it can be primitive collection.

            ParameterExpression element = Expression.Parameter(elementType, "$it");

            Expression projection;
            // expression
            //      new Wrapper { }
            if (structuredType != null)
            {
                projection = ProjectElement(element, selectExpandClause, structuredType, navigationSource);
            }
            else
            {
                projection = element;
            }

            // expression
            //      (ElementType element) => new Wrapper { }
            LambdaExpression selector = Expression.Lambda(projection, element);

            if (orderByClause != null)
            {
                source = AddOrderByQueryForSource(source, orderByClause, elementType);
            }

            bool hasTopValue = topOption != null && topOption.HasValue;
            bool hasSkipvalue = skipOption != null && skipOption.HasValue;

            IEdmEntityType entityType = structuredType as IEdmEntityType;
            if (entityType != null)
            {
                if (_settings.PageSize.HasValue || modelBoundPageSize.HasValue || hasTopValue || hasSkipvalue)
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

                    if (orderByClause == null)
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
                }
            }

            if (hasSkipvalue)
            {
                Contract.Assert(skipOption.Value <= Int32.MaxValue);
                source = ExpressionHelpers.Skip(source, (int)skipOption.Value, elementType,
                    _settings.EnableConstantParameterization);
            }

            if (hasTopValue)
            {
                Contract.Assert(topOption.Value <= Int32.MaxValue);
                source = ExpressionHelpers.Take(source, (int)topOption.Value, elementType,
                    _settings.EnableConstantParameterization);
            }

            if (_settings.PageSize.HasValue || modelBoundPageSize.HasValue || hasTopValue || hasSkipvalue)
            {
                // don't page nested collections if EnableCorrelatedSubqueryBuffering is enabled
                if (!_settings.EnableCorrelatedSubqueryBuffering)
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

            // Avoid calling source.Select($it => $it).ToList() on array types.
            if (source.Type.IsArray)
            {
                if (_settings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // source == null ? null : projectedCollection
                    return Expression.Condition(
                           test: Expression.Equal(source, Expression.Constant(null)),
                           ifTrue: Expression.Constant(null, source.Type),
                           ifFalse: source);
                }
                else
                {
                    return source;
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
